using System.Collections.Generic;
using Utf8Json;

namespace ERGLauncher.Core
{
    /// <summary>
    /// Game brand.
    /// </summary>
    public class Brand : Item
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="products">Products</param>
        [SerializationConstructor]
        public Brand(ICollection<Product> products)
        {
            this.Products = products;
        }

        /// <summary>
        /// Products.
        /// </summary>
        public ICollection<Product> Products { get; }
    }
}
