namespace E_Commers.DTO
{
    public class CategoryDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<int> ProductIds { get; set; } // قائمة بالـ ProductIds لربطها بالفئة

    }
}
