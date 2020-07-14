using System.Collections.Generic;
using Utf8Json;

namespace ERGLauncher.Core
{
    /// <summary>
    /// Root item.
    /// </summary>
    public class RootItem : Item
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="brands">Brands</param>
        [SerializationConstructor]
        public RootItem(ICollection<Brand> brands)
        {
            this.Brands = brands;
        }

        /// <summary>
        /// Brands.
        /// </summary>
        public ICollection<Brand> Brands { get; }
    }
}
