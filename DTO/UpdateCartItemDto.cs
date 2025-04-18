﻿using System.ComponentModel.DataAnnotations;

namespace E_Commers.DTO
{
    public class UpdateCartItemDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
