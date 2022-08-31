namespace AutoBuilder.AutoBuilder
{
    public class OrderBuilder : IOrderBuilder
    {
        private OrderModel order;
        public OrderModel BuildOrder()
        {
            return order;
        }

        public IOrderBuilder CreateOrder(int id)
        {
            order = new OrderModel { Id = id };

            return this;
        }

        public IOrderBuilder WithName(string name)
        {
            order.Name = name;

            return this;
        }
    }
}