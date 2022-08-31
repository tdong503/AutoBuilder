namespace AutoBuilder.AutoBuilder
{
    public interface IOrderBuilder
    {
        public OrderModel BuildOrder();

        public IOrderBuilder CreateOrder(int id);

        public IOrderBuilder WithName(string name);
    }
}