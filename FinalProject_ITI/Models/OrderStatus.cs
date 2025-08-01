namespace FinalProject_ITI.Models;

public enum OrderStatus
{
    Available, //0
    OutForDelivery,//1
    Delivered,//2 for done oders
    Cancelled,//3 BrandReceived considered cancelled
    DeliveryBrandHandingRequest,//4
    DeliveryUserHandingRequest,//5
    UserDeliveryHandingRequest,//6
    Returning//7
}