namespace AutoBuilder.AutoBuilder
{
    public static class OrderBuilderFactory
    {
        public static IOrderBuilder CreateOrder(int id)
        {
            var orderBuilder = new OrderBuilder();
            return orderBuilder.CreateOrder(id);
        }
    }
}