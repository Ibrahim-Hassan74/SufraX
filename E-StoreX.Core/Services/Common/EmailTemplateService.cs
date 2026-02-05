using EStoreX.Core.Domain.Entities.Orders;
using System.Text;

namespace EStoreX.Core.Services.Common
{
    public static class EmailTemplateService
    {
        public static string GetConfirmationEmailTemplate(string? confirmationLink)
        {
            return $@"
    <html lang='en'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <style>
            body {{
                background-color: #f9fafb;
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                margin: 0;
                padding: 0;
            }}
            .container {{
                max-width: 600px;
                margin: 40px auto;
                background: #fff;
                border-radius: 12px;
                box-shadow: 0 8px 20px rgba(0,0,0,0.05);
                overflow: hidden;
            }}
            .header {{
                background-color: #0e7490;
                padding: 24px;
                text-align: center;
                color: #ffffff;
                font-size: 24px;
                font-weight: bold;
                letter-spacing: 1px;
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
                margin-bottom: 24px;
            }}
            .cta {{
                text-align: center;
            }}
            .cta a {{
                display: inline-block;
                padding: 12px 28px;
                background-color: #0e7490;
                color: #ffffff;
                text-decoration: none;
                border-radius: 6px;
                font-weight: 600;
                transition: background-color 0.3s ease;
            }}
            .cta a:hover {{
                background-color: #0c637a;
            }}
            .footer {{
                padding: 16px;
                text-align: center;
                font-size: 12px;
                color: #94a3b8;
                border-top: 1px solid #e2e8f0;
                background-color: #f1f5f9;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>E-StoreX</div>
            <div class='body'>
                <h2>Confirm Your Email</h2>
                <p>Hi there,</p>
                <p>Thanks for signing up with <strong>E-StoreX</strong>. Click the button below to confirm your email address and get started:</p>
                <div class='cta'>
                    <a href='{confirmationLink}'>Confirm Email</a>
                </div>
                <p>If you didn’t create this account, you can safely ignore this email.</p>
            </div>
            <div class='footer'>&copy; {DateTime.Now.Year} E-StoreX. Developed by Ibrahim Hassan.</div>
        </div>
    </body>
    </html>";
        }

        public static string GetPasswordResetEmailTemplate(string? resetLink)
        {
            return $@"
    <html lang='en'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <style>
            body {{
                background-color: #f9fafb;
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                margin: 0;
                padding: 0;
            }}
            .container {{
                max-width: 600px;
                margin: 40px auto;
                background: #fff;
                border-radius: 12px;
                box-shadow: 0 8px 20px rgba(0,0,0,0.05);
                overflow: hidden;
            }}
            .header {{
                background-color: #e11d48;
                padding: 24px;
                text-align: center;
                color: #ffffff;
                font-size: 24px;
                font-weight: bold;
                letter-spacing: 1px;
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
                margin-bottom: 24px;
            }}
            .cta {{
                text-align: center;
            }}
            .cta a {{
                display: inline-block;
                padding: 12px 28px;
                background-color: #e11d48;
                color: #ffffff;
                text-decoration: none;
                border-radius: 6px;
                font-weight: 600;
                transition: background-color 0.3s ease;
            }}
            .cta a:hover {{
                background-color: #be123c;
            }}
            .footer {{
                padding: 16px;
                text-align: center;
                font-size: 12px;
                color: #94a3b8;
                border-top: 1px solid #e2e8f0;
                background-color: #f1f5f9;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>E-StoreX</div>
            <div class='body'>
                <h2>Reset Your Password</h2>
                <p>We received a request to reset the password for your E-StoreX account. Click the button below to continue:</p>
                <div class='cta'>
                    <a href='{resetLink}'>Reset Password</a>
                </div>
                <p>If you didn’t request a password reset, you can safely ignore this email. This link is valid for 10 minutes.</p>
            </div>
            <div class='footer'>&copy; {DateTime.Now.Year} E-StoreX. Developed by Ibrahim Hassan.</div>
        </div>
    </body>
    </html>";
        }

        public static string GetWeeklyNewsletterTemplate(string userName)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
  <head>
    <meta charset='UTF-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>E-StoreX Weekly Newsletter</title>
    <style>
      body {{
        background-color: #f4f6f9;
        font-family: 'Segoe UI', Arial, sans-serif;
        margin: 0;
        padding: 0;
        color: #1e293b;
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
        border-left: 5px solid #f97316;
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
  <body>
    <div class='container'>
      <div class='header'>✨ E-StoreX Weekly Newsletter</div>
      <div class='body'>
        <h2>Hi {userName}, 👋</h2>
        <p>
          We’re thrilled to share the latest updates and highlights from
          <strong>E-StoreX</strong> this week.
        </p>

        <div class='highlight'>
          🌟 Exciting new features are coming soon to improve your
          experience.<br />
          ⚡ Faster performance & smoother navigation across the platform.<br />
          🤝 Thanks for being part of our growing community!
        </div>

        <p>
          Stay tuned — more updates, tips, and exclusive insights are on the way
          🚀
        </p>
        <div class='cta'>
          <a href='https://estorex.runasp.net/swagger/index.html'>Open Dashboard</a>
        </div>
      </div>
      <div class='footer'>
        &copy; {DateTime.Now.Year} E-StoreX. Developed by Ibrahim Hassan.
      </div>
    </div>
  </body>
</html>";
        }

        public static string GetOrderConfirmationEmailTemplate(Order order)
        {
            var itemsBuilder = new StringBuilder();
            foreach (var item in order.OrderItems)
            {
                itemsBuilder.Append($@"
                <tr>
                    <td>{item.ProductName}</td>
                    <td><img src='{item.MainImage}' alt='{item.ProductName}' style='width:50px; border-radius:6px;'/></td>
                    <td>{item.Quantity}</td>
                    <td>{item.Price:C}</td>
                </tr>");
            }
            var discountRow = string.Empty;
            if (!string.IsNullOrEmpty(order.DiscountCode))
            {
                discountRow = $@"<p><strong>Discount:</strong> {order.DiscountCode} (-{order.DiscountValue:C})</p>";
            }

            return $@"
<html lang='en'>
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
            text-align: left;
        }}
        table th {{
            background-color: #f1f5f9;
        }}
        .total {{
            text-align: right;
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
<body>
    <div class='container'>
        <div class='header'>E-StoreX</div>
        <div class='body'>
            <h2>Thank you for your order!</h2>
            <p>Hi {order.Buyer.DisplayName},</p>
            <p>Your order has been successfully placed on <strong>{order.OrderDate:MMMM dd, yyyy}</strong>.</p>

            <p><strong>Order ID:</strong> {order.Id}</p>
            <p><strong>Shipping Address:</strong> {order.ShippingAddress?.Street}, {order.ShippingAddress?.City}</p>
            <p><strong>Delivery Method:</strong> {order.DeliveryMethod?.Name} ({order.DeliveryMethod?.Price:C})</p>

            <table>
                <thead>
                    <tr>
                        <th>Product</th>
                        <th>Image</th>
                        <th>Qty</th>
                        <th>Price</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsBuilder}
                </tbody>
            </table>

            {discountRow}

            <p class='total'>Total: {order.GetTotal() - order.DiscountValue:C}</p>

            <p>If you have any questions, just reply to this email — we’re happy to help!</p>
        </div>
        <div class='footer'>
            &copy; {DateTime.Now.Year} E-StoreX. Developed by Ibrahim Hassan.
        </div>
    </div>
</body>
</html>";
        }

        public static string GetPaymentFailedEmailTemplate(Order order)
        {
            var itemsBuilder = new StringBuilder();
            foreach (var item in order.OrderItems)
            {
                itemsBuilder.Append($@"
                <tr>
                    <td>{item.ProductName}</td>
                    <td><img src='{item.MainImage}' alt='{item.ProductName}' style='width:50px; border-radius:6px;'/></td>
                    <td>{item.Quantity}</td>
                    <td>{item.Price:C}</td>
                </tr>");
            }

            return $@"
<html lang='en'>
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
            text-align: left;
        }}
        table th {{
            background-color: #f1f5f9;
        }}
        table td img {{
            max-width: 50px;
            border-radius: 6px;
        }}
        .total {{
            text-align: right;
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
<body>
    <div class='container'>
        <div class='header'>Payment Failed ❌</div>
        <div class='body'>
            <h2>We're sorry, {order.BuyerEmail}.</h2>
            <p>Your payment for order <strong>#{order.Id}</strong> on <strong>{order.OrderDate:MMMM dd, yyyy}</strong> did not go through.</p>
            <p>Please check your payment details and try again, or use a different payment method.</p>

            <h3>Order Summary</h3>
            <table>
                <thead>
                    <tr>
                        <th>Product</th>
                        <th>Image</th>
                        <th>Qty</th>
                        <th>Price</th>
                    </tr>
                </thead>
                <tbody>
                    {itemsBuilder}
                </tbody>
            </table>

            <p class='total'>Total: {order.GetTotal() - order.DiscountValue:C}</p>

            <p>If the issue persists, please contact our support team and we’ll be happy to help you.</p>
        </div>
        <div class='footer'>
            &copy; {DateTime.Now.Year} E-StoreX. Developed by Ibrahim Hassan.
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
            decimal exampleOrderAmount = 500)
        {
            string discountText = "";

            if (percentage.HasValue && percentage.Value > 0)
            {
                var exampleSaving = exampleOrderAmount * (percentage.Value / 100);
                discountText = $"Save {percentage.Value}% on your next order " +
                               $"<br><small>(e.g. save {exampleSaving:C} on a {exampleOrderAmount:C} order)</small>";
            }
            else
            {
                discountText = "Special discount just for you!";
            }

            return $@"
<html lang='en'>
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
<body>
  <div class='container'>
    <div class='header'>Exclusive Discount for You 🎉</div>
    <div class='body'>
      <h2>Hi {userName},</h2>
      <p>
        As a valued member of <strong>E-StoreX</strong>, we’re excited to offer you a special discount!
      </p>

      <div class='discount-box'>
        <h3>{discountText}</h3>
        <div class='discount-code'>{discountCode}</div>
        <p>Valid until: {expiryDate:MMMM dd, yyyy}</p>
      </div>

      <p>
        Don’t miss this limited-time offer. Use the code at checkout and enjoy your savings 🚀
      </p>

      <div class='cta'>
        <a href='https://estorex.runasp.net/'>Shop Now</a>
      </div>
    </div>
    <div class='footer'>
      &copy; {DateTime.Now.Year} E-StoreX. Developed by Ibrahim Hassan.
    </div>
  </div>
</body>
</html>";
        }
        public static string GetDailySalesReportTemplate(DateTime startDate, DateTime endDate)
        {
            return $@"
<html>
  <body style='font-family:Segoe UI, sans-serif; background:#f9fafb; padding:20px;'>
    <div style='max-width:600px;margin:auto;background:#fff;border-radius:12px;padding:20px;box-shadow:0 4px 10px rgba(0,0,0,0.05)'>
      <h2 style='color:#2563eb'>📊 Daily Sales Report</h2>
      <p>Hello Admin,</p>
      <p>Please find attached the sales report for the period:</p>
      <ul>
        <li><strong>From:</strong> {startDate:yyyy-MM-dd}</li>
        <li><strong>To:</strong> {endDate:yyyy-MM-dd}</li>
      </ul>
      <p>You can download and review the Excel file attached below.</p>
      <p style='margin-top:30px;font-size:12px;color:#94a3b8;text-align:center'>
        &copy; {DateTime.Now.Year} E-StoreX. Developed by Ibrahim Hassan.
      </p>
    </div>
  </body>
</html>";
        }

        public static string GetTeaserEmailTemplate(string userName)
        {
            return $@"
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ margin: 0; padding: 0; background-color: #f1f5f9; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; }}
        .wrapper {{ width: 100%; background-color: #f1f5f9; padding: 30px 0; }}
        .main-card {{ width: 95%; max-width: 550px; background-color: #ffffff; margin: 0 auto; border-radius: 24px; overflow: hidden; box-shadow: 0 10px 30px rgba(2, 159, 174, 0.12); border: 1px solid #e2e8f0; }}
        
        .hero-header {{ background: linear-gradient(135deg, #029fae 0%, #007d8a 100%); padding: 50px 20px; text-align: center; }}
        .brand-name {{ color: #ffffff; font-size: 30px; font-weight: 800; letter-spacing: 3px; margin: 0; text-transform: uppercase; }}
        
        .content-area {{ padding: 45px 35px; text-align: center; }}
        .headline {{ font-size: 30px; font-weight: 800; color: #1e293b; line-height: 1.2; margin-bottom: 20px; letter-spacing: -0.5px; }}
        .sub-text {{ font-size: 16px; line-height: 1.7; color: #475569; margin-bottom: 25px; }}
        
        .mystery-box {{ background-color: #f0fdfa; border-radius: 20px; padding: 35px 20px; border: 2px dashed #029fae; margin: 30px 0; }}
        .secret-text {{ font-weight: 700; color: #029fae; font-size: 22px; text-transform: uppercase; letter-spacing: 1.5px; }}
        
        .btn-container {{ margin-top: 35px; }}
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
<body>
    <div class='wrapper'>
        <div class='main-card'>
            <div class='hero-header'>
                <h1 class='brand-name'>E-StoreX</h1>
            </div>
            
            <div class='content-area'>
                <h2 class='headline'>Shhh... It's a Secret. 😉</h2>
                <p class='sub-text'>Hi {userName},</p>
                <p class='sub-text'>
                    Something is happening behind closed doors at <strong>E-StoreX</strong>. 
                    We aren't ready to show the world yet, but we wanted <strong>you</strong> to be the first to know that it's coming.
                </p>

                <div class='mystery-box'>
                    <span class='secret-text'>Coming Soon</span>
                    <p style='margin-top:12px; color: #64748b; font-size: 14px; line-height: 1.4;'>No spoilers. No leaks.<br>Just greatness.</p>
                </div>

                <p class='sub-text' style='font-style: italic; font-size: 14px;'>
                    Keep this email safe. You’ll need it when the clock hits zero.
                </p>

                <div class='btn-container'>
                    <a href='#' class='primary-btn'>I'm Ready</a>
                </div>
            </div>

            <div class='footer'>
                <strong>E-StoreX Mystery Launch</strong><br>
                Curated by Ibrahim Hassan<br>
                &copy; {DateTime.Now.Year} All Rights Reserved.
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}