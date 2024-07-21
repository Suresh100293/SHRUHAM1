namespace Common.Enums
{
    public enum SourceDocument
    {
        Direct = 1,
        StockInTransit = 2,
        Requisition = 3,
        GoodsInwardNote = 4,
        WorkOrder = 5,
        SalesOrder = 6
    }

    public enum MfgType
    {
        Produced = 1,
        Consumed = 2,
        Waste = 3,
        Loss = 4
    }

    public enum WorkPriority
    {
        Normal = 1,
        Low = 2,
        Medium_High = 3,
        High = 4,
        Very_High = 5
    }

    public enum PaymentMode
    {
        Cash = 0,
        Cheque = 1,
        Transfer = 2,
        Other = 3,
        CreditCard = 4,
    }



}