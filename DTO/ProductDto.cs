using System.ComponentModel.DataAnnotations;

namespace E_Commers.DTO
{
    public class ProductDto
    {
        public class ProductDTO
        {
            // اسم المنتج
            public string ProductName { get; set; }

            // الوصف
            public string ProductDescription { get; set; }

            // السعر
            public decimal Price { get; set; }

            // الكمية في المخزن
            public int StockQuantity { get; set; }

            // الصورة الرئيسية
            public string ProductImage { get; set; } // هنا ممكن استخدام مسار الصورة أو رابط

            // رمز المنتج (SKU)
            public string SKU { get; set; }

            // التصنيفات (فئات المنتج)

            // السعر الذي يحدده البائع
            public decimal SellerPrice { get; set; }

            // الكمية المتاحة عند البائع
            public int SellerStockQuantity { get; set; }
            public int CategoryId { get; set; }  // Added CategoryId

        }
    }
}
    
        //public string Name { get; set; }
        //public string Description { get; set; }
        //public decimal Price { get; set; }
        //public string Sku { get; set; }
        //public int StockQuantity { get; set; }
        //public List<int> Categories { get; set; }
        //public string MainImageUrl { get; set; }
        //public List<ProductAttributeDto> Attributes { get; set; }
        //public decimal SellerPrice { get; set; }
        //public int SellerStockQuantity { get; set; }
        //public List<WarehouseInventoryDto> WarehouseInventories { get; set; }
        

    //}
    //public class ProductAttributeDto
    //{
    //    public string Key { get; set; }
    //    public string Value { get; set; }
    //}
    //public class WarehouseInventoryDto
    //{
    //    public int WarehouseId { get; set; }
    //}

