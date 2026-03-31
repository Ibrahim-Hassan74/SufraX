using EStoreX.Core.Domain.Entities.Orders;
using System.Text;
using System.Globalization;
using Res = EStoreX.Core.Resources.Services.Common.EmailTemplateService;

namespace EStoreX.Core.Services.Common
{
    public static class EmailTemplateService
    {
        private static (string Lang, string Dir, string Align) GetHtmlAttributes(string? culture = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                try
                {
                    var cult = new CultureInfo(culture);
                    CultureInfo.CurrentUICulture = cult;
                    CultureInfo.CurrentCulture = cult;
                }
                catch { /* Fallback to default */ }
            }

            var isAr = CultureInfo.CurrentUICulture.Name.StartsWith("ar", StringComparison.OrdinalIgnoreCase);
            return isAr ? ("ar", "rtl", "right") : ("en", "ltr", "left");
        }
        public static string GetConfirmationEmailTemplate(string? confirmationLink, string? culture = null)
        {
            var (lang, dir, align) = GetHtmlAttributes(culture);

            return $@"
<html lang='{lang}' dir='{dir}'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>

<body dir='{dir}' style='margin:0;padding:0;background-color:#f9fafb;
    font-family:Segoe UI,Tahoma,Geneva,Verdana,sans-serif;
    direction:{dir};text-align:{align};'>

    <div dir='{dir}' style='max-width:600px;margin:40px auto;background:#ffffff;
        border-radius:12px;box-shadow:0 8px 20px rgba(0,0,0,0.05);
        overflow:hidden;text-align:{align};direction:{dir};'>

        <!-- Header -->
        <div style='background-color:#0e7490;padding:24px;
            text-align:center;color:#ffffff;font-size:24px;
            font-weight:bold;letter-spacing:1px;'>
            E-StoreX
        </div>

        <!-- Body -->
        <div style='padding:32px 24px;text-align:{align};direction:{dir};'>

            <h2 style='margin-bottom:16px;font-size:22px;color:#0f172a;'>
                {Res.Confirmation_Title}
            </h2>

            <p style='color:#475569;font-size:15px;line-height:1.6;margin-bottom:16px;'>
                {Res.Confirmation_Hi}
            </p>

            <p style='color:#475569;font-size:15px;line-height:1.6;margin-bottom:24px;'>
                {Res.Confirmation_Body}
            </p>

            <div style='text-align:center;margin-bottom:24px;'>
                <a href='{confirmationLink}'
                   style='display:inline-block;padding:12px 28px;
                   background-color:#0e7490;color:#ffffff;
                   text-decoration:none;border-radius:6px;
                   font-weight:600;'>
                    {Res.Confirmation_Button}
                </a>
            </div>

            <p style='color:#475569;font-size:14px;line-height:1.6;'>
                {Res.Confirmation_Ignore}
            </p>

        </div>

        <!-- Footer -->
        <div style='padding:16px;text-align:center;font-size:12px;
            color:#94a3b8;border-top:1px solid #e2e8f0;
            background-color:#f1f5f9;direction:{dir};'>

            {string.Format(Res.Footer_Copyright, DateTime.Now.Year)}

        </div>

    </div>

</body>
</html>";
        }

        public static string GetPasswordResetEmailTemplate(string? resetLink, string? culture = null)
        {
            var (lang, dir, align) = GetHtmlAttributes(culture);

            return $@"
<html lang='{lang}' dir='{dir}'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>

<body dir='{dir}' style='margin:0;padding:0;background-color:#f9fafb;
    font-family:Segoe UI,Tahoma,Geneva,Verdana,sans-serif;
    direction:{dir};text-align:{align};'>

    <div dir='{dir}' style='max-width:600px;margin:40px auto;background:#ffffff;
        border-radius:12px;box-shadow:0 8px 20px rgba(0,0,0,0.05);
        overflow:hidden;text-align:{align};direction:{dir};'>

        <!-- Header -->
        <div style='background-color:#e11d48;padding:24px;
            text-align:center;color:#ffffff;font-size:24px;
            font-weight:bold;letter-spacing:1px;'>
            E-StoreX
        </div>

        <!-- Body -->
        <div style='padding:32px 24px;text-align:{align};direction:{dir};'>

            <h2 style='margin-bottom:16px;font-size:22px;color:#0f172a;'>
                {Res.PasswordReset_Title}
            </h2>

            <p style='color:#475569;font-size:15px;line-height:1.6;margin-bottom:24px;'>
                {Res.PasswordReset_Body}
            </p>

            <div style='text-align:center;margin-bottom:24px;'>
                <a href='{resetLink}' 
                   style='display:inline-block;padding:12px 28px;
                   background-color:#e11d48;color:#ffffff;
                   text-decoration:none;border-radius:6px;
                   font-weight:600;'>
                    {Res.PasswordReset_Button}
                </a>
            </div>

            <p style='color:#475569;font-size:14px;line-height:1.6;'>
                {Res.PasswordReset_Ignore}
            </p>

        </div>

        <!-- Footer -->
        <div style='padding:16px;text-align:center;font-size:12px;
            color:#94a3b8;border-top:1px solid #e2e8f0;
            background-color:#f1f5f9;direction:{dir};'>

            {string.Format(Res.Footer_Copyright, DateTime.Now.Year)}

        </div>

    </div>

</body>
</html>";
        }

        public static string GetWeeklyNewsletterTemplate(string userName, string? culture = null)
        {
            var (lang, dir, align) = GetHtmlAttributes(culture);
            return $@"
<!DOCTYPE html>
<html lang='{lang}' dir='{dir}'>
  <head>
    <meta charset='UTF-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>{Res.Newsletter_Title}</title>
    <style>
      body {{
        background-color: #f4f6f9;
        font-family: 'Segoe UI', Arial, sans-serif;
        margin: 0;
        padding: 0;
        color: #1e293b;
        text-align: {align};
      }}
      .container {{
        max-width: 650px;
        width: 95%;
        margin: 20px auto;
        background: #ffffff;
        border-radius: 14px;
        box-shadow: 0 6px 18px rgba(0, 0, 0, 0.06);
        overflow: hidden;
      }}
      .header {{
        background: linear-gradient(135deg, #0f172a, #2563eb);
        padding: 32px 20px;
        text-align: center;
        color: #ffffff;
        font-size: 28px;
        font-weight: 700;
        letter-spacing: 0.5px;
      }}
      .body {{
        padding: 36px 28px;
        font-size: 18px;
      }}
      .body h2 {{
        margin-bottom: 18px;
        font-size: 26px;
        font-weight: 600;
        color: #0f172a;
      }}
      .body p {{
        color: #475569;
        font-size: 18px;
        line-height: 1.8;
        margin-bottom: 22px;
      }}
      .highlight {{
        background-color: #fff7ed;
        border-left: {(dir == "rtl" ? "0" : "5px")} solid #f97316;
        border-right: {(dir == "rtl" ? "5px" : "0")} solid #f97316;
        padding: 18px 20px;
        border-radius: 8px;
        margin-bottom: 26px;
        font-size: 16px;
        color: #7c2d12;
      }}
      .cta {{
        text-align: center;
        margin: 30px 0;
      }}
      .cta a {{
        display: inline-block;
        padding: 14px 32px;
        background-color: #2563eb;
        color: #ffffff;
        text-decoration: none;
        border-radius: 8px;
        font-weight: 600;
        font-size: 16px;
        transition: all 0.3s ease;
      }}
      .cta a:hover {{
        background-color: #1d4ed8;
      }}
      .footer {{
        padding: 20px;
        text-align: center;
        font-size: 16px;
        color: #475569;
        border-top: 1px solid #e2e8f0;
        background-color: #f8fafc;
      }}

      /* Responsive styles */
      @media only screen and (max-width: 480px) {{
        .header {{
          font-size: 24px;
          padding: 24px 15px;
        }}
        .body {{
          padding: 24px 15px;
          font-size: 16px;
        }}
        .body h2 {{
          font-size: 22px;
        }}
        .highlight {{
          padding: 14px 15px;
          font-size: 14px;
        }}
        .cta a {{
          padding: 12px 24px;
          font-size: 15px;
        }}
        .footer {{
          font-size: 14px;
        }}
      }}
    </style>
  </head>
  <body dir='{dir}'>
    <div class='container'>
      <div class='header'>✨ {Res.Newsletter_Title}</div>
      <div class='body'>
        <h2>{string.Format(Res.Newsletter_Greeting, userName)}</h2>
        <p>{Res.Newsletter_Body}</p>

        <div class='highlight'>
          {Res.Newsletter_Highlight_Features}<br />
          {Res.Newsletter_Highlight_Performance}<br />
          {Res.Newsletter_Highlight_Community}
        </div>

        <p>{Res.Newsletter_StayTuned}</p>
        <div class='cta'>
          <a href='https://estorex.runasp.net/swagger/index.html'>{Res.Newsletter_OpenDashboard}</a>
        </div>
      </div>
      <div class='footer'>
        {string.Format(Res.Footer_Copyright, DateTime.Now.Year)}
      </div>
    </div>
  </body>
</html>";
        }

        public static string GetOrderConfirmationEmailTemplate(Order order, string? culture = null)
        {
            var (lang, dir, align) = GetHtmlAttributes(culture);
            var itemsBuilder = new StringBuilder();
            foreach (var item in order.OrderItems)
            {
                itemsBuilder.Append($@"
                <tr>
                    <td>{(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? item.ProductNameAr : item.ProductNameEn)}</td>
                    <td><img src='{item.MainImage}' alt='{(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? item.ProductNameAr : item.ProductNameEn)}' style='width:50px; border-radius:6px;'/></td>
                    <td>{item.Quantity}</td>
                    <td>{item.Price:C}</td>
                </tr>");
            }
            var discountRow = string.Empty;
            if (!string.IsNullOrEmpty(order.DiscountCode))
            {
                discountRow = $@"<p><strong>{Res.OrderConfirmation_Discount}</strong> {order.DiscountCode} (-{order.DiscountValue:C})</p>";
            }

            return $@"
<html lang='{lang}' dir='{dir}'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            background-color: #f9fafb;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 0;
            color: #1e293b;
            text-align: {align};
        }}
        .container {{
            max-width: 650px;
            margin: 30px auto;
            background: #ffffff;
            border-radius: 14px;
            box-shadow: 0 6px 18px rgba(0,0,0,0.06);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #2563eb, #0f172a);
            padding: 28px 20px;
            text-align: center;
            color: #ffffff;
            font-size: 24px;
            font-weight: 700;
            letter-spacing: 0.5px;
        }}
        .body {{
            padding: 28px 22px;
        }}
        .body h2 {{
            margin-bottom: 14px;
            font-size: 22px;
            color: #0f172a;
        }}
        .body p {{
            color: #475569;
            font-size: 15px;
            line-height: 1.6;
            margin-bottom: 14px;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }}
        table th, table td {{
            border: 1px solid #e5e7eb;
            padding: 10px;
            font-size: 14px;
            text-align: {align};
        }}
        table th {{
            background-color: #f1f5f9;
        }}
        .total {{
            text-align: {(dir == "rtl" ? "left" : "right")};
            font-size: 16px;
            font-weight: bold;
            margin-top: 15px;
        }}
        .footer {{
            padding: 16px;
            text-align: center;
            font-size: 13px;
            color: #94a3b8;
            border-top: 1px solid #e2e8f0;
            background-color: #f8fafc;
        }}
    </style>
</head>
<body dir='{dir}'>
    <div class='container'>
        <div class='header'>E-StoreX</div>
        <div class='body'>
            <h2>{Res.OrderConfirmation_Title}</h2>
            <p>{string.Format(Res.OrderConfirmation_Hi, order.Buyer.DisplayName)}</p>
            <p>{string.Format(Res.OrderConfirmation_PlacedOn, order.OrderDate.ToString("MMMM dd, yyyy"))}</p>

            <p><strong>{Res.OrderConfirmation_OrderID}</strong> {order.Id}</p>
            <p><strong>{Res.OrderConfirmation_ShippingAddress}</strong> {order.ShippingAddress?.Street}, {order.ShippingAddress?.City}</p>
            <p><strong>{Res.OrderConfirmation_DeliveryMethod}</strong> {(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? order.DeliveryMethod?.NameAr : order.DeliveryMethod?.NameEn)} ({order.DeliveryMethod?.Price:C})</p>

            <table>
                <thead>
                    <tr>
                        <th>{Res.OrderConfirmation_Table_Product}</th>
                        <th>{Res.OrderConfirmation_Table_Image}</th>
                        <th>{Res.OrderConfirmation_Table_Qty}</th>
                        <th>{Res.OrderConfirmation_Table_Price}</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsBuilder}
                </tbody>
            </table>

            {discountRow}

            <p class='total'>{Res.OrderConfirmation_Total} {order.GetTotal() - order.DiscountValue:C}</p>

            <p>{Res.OrderConfirmation_Questions}</p>
        </div>
        <div class='footer'>
            {string.Format(Res.Footer_Copyright, DateTime.Now.Year)}
        </div>
    </div>
</body>
</html>";
        }

        public static string GetPaymentFailedEmailTemplate(Order order, string? culture = null)
        {
            var (lang, dir, align) = GetHtmlAttributes(culture);
            var itemsBuilder = new StringBuilder();
            foreach (var item in order.OrderItems)
            {
                itemsBuilder.Append($@"
                <tr>
                    <td>{(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? item.ProductNameAr : item.ProductNameEn)}</td>
                    <td><img src='{item.MainImage}' alt='{(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? item.ProductNameAr : item.ProductNameEn)}' style='width:50px; border-radius:6px;'/></td>
                    <td>{item.Quantity}</td>
                    <td>{item.Price:C}</td>
                </tr>");
            }

            return $@"
<html lang='{lang}' dir='{dir}'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            background-color: #f9fafb;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 0;
            color: #1e293b;
            text-align: {align};
        }}
        .container {{
            max-width: 650px;
            margin: 30px auto;
            background: #ffffff;
            border-radius: 14px;
            box-shadow: 0 6px 18px rgba(0,0,0,0.06);
            overflow: hidden;
        }}
        .header {{
            background: #dc2626;
            padding: 28px 20px;
            text-align: center;
            color: #ffffff;
            font-size: 24px;
            font-weight: 700;
            letter-spacing: 0.5px;
        }}
        .body {{
            padding: 28px 22px;
        }}
        .body h2 {{
            margin-bottom: 14px;
            font-size: 22px;
            color: #b91c1c;
        }}
        .body p {{
            color: #475569;
            font-size: 15px;
            line-height: 1.6;
            margin-bottom: 14px;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }}
        table th, table td {{
            border: 1px solid #e5e7eb;
            padding: 10px;
            font-size: 14px;
            text-align: {align};
        }}
        table th {{
            background-color: #f1f5f9;
        }}
        table td img {{
            max-width: 50px;
            border-radius: 6px;
        }}
        .total {{
            text-align: {(dir == "rtl" ? "left" : "right")};
            font-size: 16px;
            font-weight: bold;
            margin-top: 15px;
        }}
        .footer {{
            padding: 16px;
            text-align: center;
            font-size: 13px;
            color: #94a3b8;
            border-top: 1px solid #e2e8f0;
            background-color: #f8fafc;
        }}
    </style>
</head>
<body dir='{dir}'>
    <div class='container'>
        <div class='header'>{Res.PaymentFailed_Title}</div>
        <div class='body'>
            <h2>{string.Format(Res.PaymentFailed_Hi, order.BuyerEmail)}</h2>
            <p>{string.Format(Res.PaymentFailed_Body, order.Id, order.OrderDate.ToString("MMMM dd, yyyy"))}</p>
            <p>{Res.PaymentFailed_Retry}</p>

            <h3>{Res.PaymentFailed_Summary}</h3>
            <table>
                <thead>
                    <tr>
                        <th>{Res.OrderConfirmation_Table_Product}</th>
                        <th>{Res.OrderConfirmation_Table_Image}</th>
                        <th>{Res.OrderConfirmation_Table_Qty}</th>
                        <th>{Res.OrderConfirmation_Table_Price}</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsBuilder}
                </tbody>
            </table>

            <p class='total'>{Res.OrderConfirmation_Total} {order.GetTotal() - order.DiscountValue:C}</p>

            <p>{Res.PaymentFailed_ContactSupport}</p>
        </div>
        <div class='footer'>
            {string.Format(Res.Footer_Copyright, DateTime.Now.Year)}
        </div>
    </div>
</body>
</html>";
        }
        public static string GetDiscountEmailTemplate(
            string userName,
            string discountCode,
            decimal? percentage,
            DateTime expiryDate,
            decimal exampleOrderAmount = 500,
            string? culture = null)
        {
            var (lang, dir, align) = GetHtmlAttributes(culture);
            string discountText = "";

            if (percentage.HasValue && percentage.Value > 0)
            {
                var exampleSaving = exampleOrderAmount * (percentage.Value / 100);
                if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar")
                {
                    discountText = $"وفر {percentage.Value}% على طلبك القادم " +
                                   $"<br><small>(مثلاً وفر {exampleSaving:C} على طلب بقيمة {exampleOrderAmount:C})</small>";
                }
                else
                {
                    discountText = $"Save {percentage.Value}% on your next order " +
                                   $"<br><small>(e.g. save {exampleSaving:C} on a {exampleOrderAmount:C} order)</small>";
                }
            }
            else
            {
                discountText = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar"
                    ? "خصم خاص لك فقط!"
                    : "Special discount just for you!";
            }

            return $@"
<html lang='{lang}' dir='{dir}'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <style>
    body {{
      background-color: #f9fafb;
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      margin: 0;
      padding: 0;
      color: #1e293b;
      text-align: {align};
    }}
    .container {{
      max-width: 600px;
      margin: 40px auto;
      background: #ffffff;
      border-radius: 14px;
      box-shadow: 0 6px 18px rgba(0,0,0,0.06);
      overflow: hidden;
    }}
    .header {{
      background: linear-gradient(135deg, #16a34a, #065f46);
      padding: 28px 20px;
      text-align: center;
      color: #ffffff;
      font-size: 26px;
      font-weight: 700;
      letter-spacing: 0.5px;
    }}
    .body {{
      padding: 32px 24px;
    }}
    .body h2 {{
      margin-bottom: 16px;
      font-size: 22px;
      color: #0f172a;
    }}
    .body p {{
      color: #475569;
      font-size: 15px;
      line-height: 1.6;
      margin-bottom: 20px;
    }}
    .discount-box {{
      text-align: center;
      background-color: #ecfdf5;
      border: 2px dashed #16a34a;
      border-radius: 10px;
      padding: 20px;
      margin: 20px 0;
    }}
    .discount-box h3 {{
      font-size: 20px;
      color: #065f46;
      margin: 0 0 10px 0;
    }}
    .discount-code {{
      font-size: 24px;
      font-weight: bold;
      color: #16a34a;
      background: #f0fdf4;
      padding: 10px 20px;
      border-radius: 6px;
      display: inline-block;
      letter-spacing: 2px;
    }}
    .cta {{
      text-align: center;
      margin: 30px 0;
    }}
    .cta a {{
      display: inline-block;
      padding: 12px 28px;
      background-color: #16a34a;
      color: #ffffff;
      text-decoration: none;
      border-radius: 6px;
      font-weight: 600;
      transition: background-color 0.3s ease;
    }}
    .cta a:hover {{
      background-color: #15803d;
    }}
    .footer {{
      padding: 16px;
      text-align: center;
      font-size: 13px;
      color: #94a3b8;
      border-top: 1px solid #e2e8f0;
      background-color: #f8fafc;
    }}
  </style>
</head>
<body dir='{dir}'>
  <div class='container'>
    <div class='header'>{Res.Discount_Title}</div>
    <div class='body'>
      <h2>{string.Format(Res.Discount_Hi, userName)}</h2>
      <p>{Res.Discount_Intro}</p>

      <div class='discount-box'>
        <h3>{discountText}</h3>
        <div class='discount-code'>{discountCode}</div>
        <p>{string.Format(Res.Discount_ValidUntil, expiryDate.ToString("MMMM dd, yyyy"))}</p>
      </div>

      <p>{Res.Discount_Body}</p>

      <div class='cta'>
        <a href='https://estorex.runasp.net/'>{Res.Discount_ShopNow}</a>
      </div>
    </div>
    <div class='footer'>
      {string.Format(Res.Footer_Copyright, DateTime.Now.Year)}
    </div>
  </div>
</body>
</html>";
        }
        public static string GetDailySalesReportTemplate(DateTime startDate, DateTime endDate, string? culture = null)
        {
            var (lang, dir, align) = GetHtmlAttributes(culture);
            return $@"
<html lang='{lang}' dir='{dir}'>
  <body dir='{dir}' style='font-family:Segoe UI, sans-serif; background:#f9fafb; padding:20px; text-align: {align};'>
    <div style='max-width:600px;margin:auto;background:#fff;border-radius:12px;padding:20px;box-shadow:0 4px 10px rgba(0,0,0,0.05)'>
      <h2 style='color:#2563eb'>{Res.SalesReport_Title}</h2>
      <p>{Res.SalesReport_Greeting}</p>
      <p>{Res.SalesReport_Body}</p>
      <ul style='list-style-position: inside; padding: 0;'>
        <li><strong>{Res.SalesReport_From}</strong> {startDate:yyyy-MM-dd}</li>
        <li><strong>{Res.SalesReport_To}</strong> {endDate:yyyy-MM-dd}</li>
      </ul>
      <p>{Res.SalesReport_Download}</p>
      <p style='margin-top:30px;font-size:12px;color:#94a3b8;text-align:center'>
        {string.Format(Res.Footer_Copyright, DateTime.Now.Year)}
      </p>
    </div>
  </body>
</html>";
        }

        public static string GetTeaserEmailTemplate(string userName, string? culture = null)
        {
            var (lang, dir, align) = GetHtmlAttributes(culture);
            return $@"
<html lang='{lang}' dir='{dir}'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ margin: 0; padding: 0; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; text-align: {align}; }}
        .wrapper {{ width: 100%; background-color: #f1f5f9; padding: 30px 0; }}
        .main-card {{ width: 95%; max-width: 550px; background-color: #ffffff; margin: 0 auto; border-radius: 24px; overflow: hidden; box-shadow: 0 10px 30px rgba(2, 159, 174, 0.12); border: 1px solid #e2e8f0; }}
        
        .hero-header {{ background: linear-gradient(135deg, #029fae 0%, #007d8a 100%); padding: 50px 20px; text-align: center; }}
        .brand-name {{ color: #ffffff; font-size: 30px; font-weight: 800; letter-spacing: 3px; margin: 0; text-transform: uppercase; }}
        
        .content-area {{ padding: 45px 35px; text-align: {align}; }}
        .headline {{ font-size: 30px; font-weight: 800; color: #1e293b; line-height: 1.2; margin-bottom: 20px; letter-spacing: -0.5px; }}
        .sub-text {{ font-size: 16px; line-height: 1.7; color: #475569; margin-bottom: 25px; }}
        
        .mystery-box {{ background-color: #f0fdfa; border-radius: 20px; padding: 35px 20px; border: 2px dashed #029fae; margin: 30px 0; }}
        .secret-text {{ font-weight: 700; color: #029fae; font-size: 22px; text-transform: uppercase; letter-spacing: 1.5px; }}
        
        .btn-container {{ margin-top: 35px; text-align: center; }}
        .primary-btn {{ background-color: #029fae; color: #ffffff !important; padding: 18px 45px; text-decoration: none; border-radius: 50px; font-weight: 700; font-size: 16px; display: inline-block; box-shadow: 0 5px 15px rgba(2, 159, 174, 0.3); }}
        
        .footer {{ padding: 35px; text-align: center; color: #94a3b8; font-size: 13px; line-height: 1.6; border-top: 1px solid #f1f5f9; }}

        @media screen and (max-width: 600px) {{
            .main-card {{ border-radius: 20px; width: 92%; }}
            .headline {{ font-size: 26px; }}
            .hero-header {{ padding: 40px 20px; }}
            .primary-btn {{ width: 85%; box-sizing: border-box; }}
        }}
    </style>
</head>
<body dir='{dir}'>
    <div class='wrapper'>
        <div class='main-card'>
            <div class='hero-header'>
                <h1 class='brand-name'>E-StoreX</h1>
            </div>
            
            <div class='content-area'>
                <h2 class='headline'>{Res.Teaser_Headline}</h2>
                <p class='sub-text'>{string.Format(Res.Teaser_Hi, userName)}</p>
                <p class='sub-text'>
                    {Res.Teaser_Body}
                </p>

                <div class='mystery-box'>
                    <span class='secret-text'>{Res.Teaser_ComingSoon}</span>
                    <p style='margin-top:12px; color: #64748b; font-size: 14px; line-height: 1.4;'>{Res.Teaser_MysteryText}</p>
                </div>

                <p class='sub-text' style='font-style: italic; font-size: 14px;'>
                    {Res.Teaser_SafeEmail}
                </p>

                <div class='btn-container'>
                    <a href='#' class='primary-btn'>{Res.Teaser_ReadyBtn}</a>
                </div>
            </div>

            <div class='footer'>
                <strong>{Res.Teaser_FooterBrand}</strong><br>
                {Res.Teaser_FooterCuratedBy}<br>
                {string.Format(Res.Footer_Copyright, DateTime.Now.Year)}
            </div>
        </div>
    </div>
</body>
</html>";
        }

        public static string GetContactMessageTemplate(
    string name,
    string email,
    string subject,
    string message,
    string? culture = null)
        {
            var (lang, dir, align) = GetHtmlAttributes(culture);
            return $@"
<html lang='{lang}' dir='{dir}'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ margin: 0; padding: 0; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; text-align: {align}; }}
        .wrapper {{ width: 100%; background-color: #f1f5f9; padding: 30px 0; }}
        .main-card {{ width: 95%; max-width: 550px; background-color: #ffffff; margin: 0 auto; border-radius: 24px; overflow: hidden; box-shadow: 0 10px 30px rgba(2, 159, 174, 0.12); border: 1px solid #e2e8f0; }}

        .hero-header {{ background: linear-gradient(135deg, #029fae 0%, #007d8a 100%); padding: 40px 20px; text-align: center; }}
        .brand-name {{ color: #ffffff; font-size: 28px; font-weight: 800; letter-spacing: 3px; margin: 0; text-transform: uppercase; }}

        .content-area {{ padding: 40px 30px; }}
        .headline {{ font-size: 24px; font-weight: 700; color: #1e293b; margin-bottom: 20px; }}
        .info-row {{ font-size: 15px; color: #475569; margin-bottom: 12px; }}
        .message-box {{ background-color: #f8fafc; border-radius: 12px; padding: 20px; border: 1px solid #e2e8f0; margin-top: 20px; white-space: pre-line; }}

        .footer {{ padding: 30px; text-align: center; color: #94a3b8; font-size: 13px; border-top: 1px solid #f1f5f9; }}
    </style>
</head>
<body>
    <div class='wrapper'>
        <div class='main-card'>

            <div class='hero-header'>
                <h1 class='brand-name'>E-StoreX</h1>
            </div>

            <div class='content-area'>
                <h2 class='headline'>{Res.ContactMessage_Title}</h2>

                <p class='info-row'><strong>{Res.ContactMessage_Table_Name}</strong> {name}</p>
                <p class='info-row'><strong>{Res.ContactMessage_Table_Email}</strong> {email}</p>
                <p class='info-row'><strong>{Res.ContactMessage_Table_Subject}</strong> {subject}</p>

                <div class='message-box'>
                    {message}
                </div>
            </div>

            <div class='footer'>
                {Res.ContactMessage_Footer}<br>
                {string.Format(Res.Footer_Copyright, DateTime.Now.Year)}
            </div>

        </div>
    </div>
</body>
</html>";
        }

    }
}