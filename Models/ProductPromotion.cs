﻿namespace E_Commers.Models
{
    public class ProductPromotion
    {
        public int ProductId { get; set; }
        public int PromotionId { get; set; }

        // Navigation properties
        public Product Product { get; set; }
        public Promotion Promotion { get; set; }
    }
}
