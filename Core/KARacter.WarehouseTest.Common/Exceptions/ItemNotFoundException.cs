namespace KARacter.WarehouseTest.Common.Exceptions
{
    [Serializable]
    public class ItemNotFoundException : Exception
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public ItemNotFoundException(string id, string name) : base($"{name} with given id ({id}) was not found")
        {
            Id = id;
            Name = name; 
        }
    }
}
