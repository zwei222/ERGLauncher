using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ControlzEx.Theming;
using DryIoc;
using ERGLauncher.Core.DialogSettings.Implementations;
using ERGLauncher.Models;
using ERGLauncher.Models.Implementations;
using ERGLauncher.Services;
using ERGLauncher.Services.Implementations;
using ERGLauncher.ViewModels;
using ERGLauncher.Views;
using Gu.Localization;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using ZLogger;

namespace ERGLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private ILogger<App>? logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        public App()
        {
            this.DispatcherUnhandledException += (sender, e) =>
            {
                this.OnFatalException(e.Exception);
            };
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                this.OnFatalException(e.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                this.OnFatalException(e.ExceptionObject as Exception);
            };
        }

        /// <inheritdoc />
        protected override Window CreateShell()
        {
            return this.Container.Resolve<MainView>();
        }

        /// <inheritdoc />
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            ThemeManager.Current.SyncTheme();
        }

        /// <inheritdoc />
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            if (containerRegistry == null)
            {
                throw new ArgumentNullException(nameof(containerRegistry));
            }

            // logger
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.ClearProviders();
#if DEBUG
                builder.SetMinimumLevel(LogLevel.Trace);
#else
                builder.SetMinimumLevel(LogLevel.Information);
#endif
                builder.AddZLoggerRollingFile(
                    (dateTimeOffset, number) => $"logs/{dateTimeOffset.ToLocalTime():yyyy-MM-dd}_{number:000}.log",
                    dateTimeOffset => dateTimeOffset.ToLocalTime().Date,
                    1024);
            });
            var container = containerRegistry.GetContainer();
            var loggerFactoryMethod =
                typeof(LoggerFactoryExtensions).GetMethod(nameof(LoggerFactoryExtensions.CreateLogger),
                    new[] { typeof(ILoggerFactory) });

            container.UseInstance(loggerFactory);
            container.Register(typeof(ILogger<>),
                made: Made.Of(request => loggerFactoryMethod!.MakeGenericMethod(request.Parent.ImplementationType)));
            this.logger = loggerFactory.CreateLogger<App>();

            // services
            containerRegistry.RegisterInstance(typeof(Dispatcher), this.Dispatcher);
            containerRegistry.RegisterSingleton<IDispatcherService, DispatcherService>();
            containerRegistry.RegisterSingleton<IResourceService, ResourceService>();
            containerRegistry.RegisterSingleton<IThemeService, ThemeService>();
            containerRegistry.RegisterSingleton<IFileService, FileService>();
            containerRegistry.RegisterSingleton<IAppSettingService, AppSettingService>();
            containerRegistry.RegisterSingleton<IGameSettingService, GameSettingService>();
            containerRegistry.RegisterInstance(DialogCoordinator.Instance);
            containerRegistry.RegisterSingleton<ICommonDialogService, CommonDialogService>();

            // models
            containerRegistry.Register<IMainModel, MainModel>();
            containerRegistry.Register<IMessageModel, MessageModel>();
            containerRegistry.Register<IAddBrandModel, AddBrandModel>();
            containerRegistry.Register<IAddProductModel, AddProductModel>();
            containerRegistry.Register<ISettingModel, SettingModel>();

            // dialogs
            containerRegistry.RegisterDialogWindow<DialogBase>();
            containerRegistry.RegisterDialog<MessageView, MessageViewModel>();
            containerRegistry.RegisterDialog<AddBrandView, AddBrandViewModel>();
            containerRegistry.RegisterDialog<AddProductView, AddProductViewModel>();
            containerRegistry.RegisterDialog<SettingView, SettingViewModel>();
        }

        /// <inheritdoc />
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();
            ViewModelLocationProvider.Register<MainView, MainViewModel>();
            ViewModelLocationProvider.Register<MessageView, MessageViewModel>();
            ViewModelLocationProvider.Register<AddBrandView, AddBrandViewModel>();
            ViewModelLocationProvider.Register<AddProductView, AddProductViewModel>();
            ViewModelLocationProvider.Register<SettingView, SettingViewModel>();
        }

        /// <summary>
        /// Called when a fatal exception occurs.
        /// </summary>
        /// <param name="exception">Exception</param>
        private void OnFatalException(Exception? exception)
        {
            if (exception != null)
            {
                this.logger?.ZLogCritical(
                    exception,
                    "[{0}] {1}",
                    DateTimeOffset.Now.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture),
                    Translator.Translate(
                        ERGLauncher.Properties.Resources.ResourceManager,
                        nameof(ERGLauncher.Properties.Resources.OnUnexpectedException),
                        CultureInfo.CurrentCulture));
            }

            var dialogService = this.Container.Resolve<IDialogService>();

            if (dialogService != null)
            {
                var dialogParameters = new DialogParameters();

                dialogParameters.Add(nameof(MessageDialogSettings), new MessageDialogSettings
                {
                    Height = 500,
                    Width = 400,
                    Message = Translator.Translate(
                        ERGLauncher.Properties.Resources.ResourceManager,
                        nameof(ERGLauncher.Properties.Resources.OnUnexpectedException),
                        CultureInfo.CurrentCulture) ?? string.Empty,
                    Details = exception?.ToString(),
                });

                try
                {
                    dialogService.ShowDialog(nameof(MessageView), dialogParameters, _ => { });
                }
                catch (Exception e)
                {
                    this.logger?.ZLogCritical(
                        e,
                        "[{0}] {1}",
                        DateTimeOffset.Now.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture),
                        Translator.Translate(
                            ERGLauncher.Properties.Resources.ResourceManager,
                            nameof(ERGLauncher.Properties.Resources.OnUnexpectedException),
                            CultureInfo.CurrentCulture));
                }
            }

            Environment.Exit(1);
        }
    }
}
