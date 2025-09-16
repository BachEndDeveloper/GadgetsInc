using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace GadgetsInc.ApiService.Functions;

public class CustomerServiceFunctions
{
    [KernelFunction, Description("Get customer support information for common issues and questions.")]
    public static string GetSupportInfo(string issueType)
    {
        var supportInfo = new Dictionary<string, string>
        {
            { "warranty", "All GadgetsInc products come with a 2-year manufacturer warranty. Contact support at support@gadgetsinc.com for warranty claims." },
            { "return", "30-day return policy for all products. Items must be in original condition. Return shipping is free for defective items." },
            { "repair", "We offer repair services for all our products. Schedule a repair appointment at gadgetsinc.com/repair or call 1-800-GADGETS." },
            { "shipping", "Free shipping on orders over $99. Standard shipping takes 3-5 business days. Express shipping available for next-day delivery." },
            { "payment", "We accept all major credit cards, PayPal, and Apple Pay. Financing options available for purchases over $500." },
            { "contact", "Customer Service: 1-800-GADGETS (1-800-423-4387), Email: support@gadgetsinc.com, Hours: Mon-Fri 8AM-8PM EST" }
        };

        var key = issueType.ToLowerInvariant();
        if (supportInfo.TryGetValue(key, out var info))
        {
            return info;
        }

        return "For general support questions, please contact our customer service team at 1-800-GADGETS or support@gadgetsinc.com. Available topics: warranty, return, repair, shipping, payment, contact.";
    }

    [KernelFunction, Description("Track order status by order number. Provides current shipping and delivery information.")]
    public static string TrackOrder(string orderNumber)
    {
        // Simulate order tracking
        var random = new Random(orderNumber.GetHashCode());
        var statuses = new[] { "Processing", "Shipped", "Out for Delivery", "Delivered" };
        var carriers = new[] { "UPS", "FedEx", "USPS" };
        
        var status = statuses[random.Next(statuses.Length)];
        var carrier = carriers[random.Next(carriers.Length)];
        var trackingNumber = $"{carrier}{random.Next(100000, 999999)}";
        
        var estimatedDelivery = DateTime.Now.AddDays(random.Next(1, 5)).ToString("MMM dd, yyyy");

        return status switch
        {
            "Processing" => $"Order {orderNumber}: Currently being processed. Estimated shipping date: {DateTime.Now.AddDays(1):MMM dd, yyyy}",
            "Shipped" => $"Order {orderNumber}: Shipped via {carrier}. Tracking: {trackingNumber}. Estimated delivery: {estimatedDelivery}",
            "Out for Delivery" => $"Order {orderNumber}: Out for delivery with {carrier}. Expected delivery today.",
            "Delivered" => $"Order {orderNumber}: Delivered successfully on {DateTime.Now.AddDays(-1):MMM dd, yyyy}",
            _ => $"Order {orderNumber}: Status unknown. Please contact customer service."
        };
    }

    [KernelFunction, Description("Generate a support ticket for customer issues. Returns ticket number and next steps.")]
    public static string CreateSupportTicket(string customerEmail, string issueDescription)
    {
        var ticketNumber = $"TK{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}";
        
        return $"Support ticket {ticketNumber} created for {customerEmail}. " +
               $"Issue: {issueDescription}. " +
               $"Our support team will respond within 24 hours. " +
               $"You can track your ticket at gadgetsinc.com/support/track/{ticketNumber}";
    }
}