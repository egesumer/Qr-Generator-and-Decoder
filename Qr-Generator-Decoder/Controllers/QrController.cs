using Microsoft.AspNetCore.Mvc;
using QRCoder;
using SkiaSharp;
using System;
using ZXing;

namespace QrCodeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrCodeController : ControllerBase
    {
        // 1. Endpoint: QR kod oluşturma ve kullanıcıya gösterme
        [HttpPost("generate")]
        public IActionResult GenerateQrCode([FromBody] string input)
        {
            try
            {
                // QR kodu oluştur
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(input, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeAsPng = qrCode.GetGraphic(20);

                // PNG dosyasını döndür
                return File(qrCodeAsPng, "image/png");
            }
            catch (Exception ex)
            {
                return BadRequest($"QR kod oluşturulurken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpPost("generate-base64")]
        public IActionResult DecodeAndDownloadQR([FromBody] string input)
        {
             try
            {
                // QR kodu oluştur
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(input, QRCodeGenerator.ECCLevel.Q);
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeAsPng = qrCode.GetGraphic(20);
                string base64Image = Convert.ToBase64String(qrCodeAsPng);

                // PNG dosyasını döndür
                return File(qrCodeAsPng, "image/png",base64Image);
            }
            catch (Exception ex)
            {
                return BadRequest($"QR kod oluşturulurken bir hata oluştu: {ex.Message}");
            }
        }

        // 2. Endpoint: QR kod çözümleme için Base64 string kullanma
         [HttpPost("decode-base64")]
        public IActionResult DecodeQrCodeFromBase64([FromBody] string base64Image)
        {
            if (string.IsNullOrEmpty(base64Image))
                return BadRequest("Geçerli bir base64 string sağlayın.");

            try
            {
                // Base64 string'den prefix'i kaldır
                if (base64Image.StartsWith("data:image/png;base64,"))
                {
                    base64Image = base64Image.Substring("data:image/png;base64,".Length);
                }

                // Base64 string'i byte dizisine dönüştür
                byte[] imageBytes = Convert.FromBase64String(base64Image);

                // Byte dizisini MemoryStream'e yükle
                using var stream = new MemoryStream(imageBytes);
                using var skBitmap = SKBitmap.Decode(stream);

                // SkiaSharp ile QR kod çözümleme
                var reader = new ZXing.BarcodeReaderGeneric
                {
                    Options = new ZXing.Common.DecodingOptions { TryHarder = true }
                };

                var result = reader.Decode(skBitmap);

                if (result != null)
                    return Ok(result.Text);
                else
                    return BadRequest("QR kod çözümleme başarısız.");
            }
            catch (Exception ex)
            {
                return BadRequest($"QR kodu çözümleme sırasında bir hata oluştu: {ex.Message}");
            }
        }
    }
}
