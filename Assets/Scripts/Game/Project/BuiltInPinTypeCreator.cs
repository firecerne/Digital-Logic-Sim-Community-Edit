using DLS.Description;

namespace DLS.Game
{
    public static class BuiltinPinTypeCreator
    {
        public static PinBitCount[] CreateBuiltInPinType()
        {
            return new PinBitCount[]
            {
                1,  4,  8
            };
        }        
    }
}