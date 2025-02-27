using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System;
using System.Drawing;

namespace SAGM.Controllers
{
    public class QRCode : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string QRCodeText)
        {
            string QrUri;
            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(QRCodeText, QRCodeGenerator.ECCLevel.Q))
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                QrUri = string.Format("data:image/png;base64,{0}", Convert.ToBase64String(qrCodeImage));
            }
            ViewBag.QrCodeUri = QrUri;

            return View();
        }
    }
}
