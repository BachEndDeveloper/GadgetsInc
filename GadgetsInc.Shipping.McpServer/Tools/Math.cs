using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace GadgetsInc.Shipping.McpServer.Tools;

public class Math
{
    [KernelFunction, Description("Addition of 2 numbers. Accept decimal and natural numbers. Return the sum of the numbers.")]
    public static double Add(double a, double b) => a + b;
    
    [KernelFunction, Description("Subtraction of 2 numbers. Accept decimal and natural numbers. Return the sum of the numbers subtracted.")]
    public static double Subtract(double a, double b) => a - b;
    
    [KernelFunction, Description("Multiplication of 2 numbers. Accept decimal and natural numbers. Return the sum of the numbers multiplied.")]
    public static double Multiply(double a, double b) => a * b;

    [KernelFunction, Description("Division of 2 numbers. Accept decimal and natural numbers. Return the sum of the numbers divided.")]
    public static double Divide(double a, double b) => a / b;
}