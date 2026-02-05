namespace EStoreX.Core.DTO.Common
{
    public class ImageCropDto
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Zoom { get; set; } = 1f;
    }
}
