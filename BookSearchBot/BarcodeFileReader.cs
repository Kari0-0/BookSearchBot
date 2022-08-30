using BarcodeLib.BarcodeReader;

namespace BookSearchBot
{
    
    public static class BarcodeFileReader
    {
        
        public static string Read(Stream file)
        {
            
            var res = BarcodeReader.read(file, BarcodeReader.EAN13);
            if (res.Length == 0)
            {
                return String.Empty;
            }
            return String.Join(" ", res);
        }
    }
}
