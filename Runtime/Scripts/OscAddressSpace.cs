using System.Text.RegularExpressions;

namespace OscCore
{
    public sealed class OscAddressSpace(int startingCapacity = OscAddressSpace.k_DefaultCapacity)
    {
        const int k_DefaultPatternCapacity = 8;
        const int k_DefaultCapacity = 16;

        internal readonly OscAddressMethods AddressToMethod = new(startingCapacity);
        
        // Keep a list of registered address patterns and the methods they're associated with just like addresses
        internal int PatternCount;
        internal Regex?[] Patterns = new Regex?[k_DefaultPatternCapacity];
        internal OscActionPair?[] PatternMethods = new OscActionPair?[k_DefaultPatternCapacity];
        
        readonly Queue<int> FreedPatternIndices = new();
        readonly Dictionary<string, int> PatternStringToIndex = [];

        public int HandlerCount => AddressToMethod.HandleToValue.Count;

        public IEnumerable<string> Addresses => AddressToMethod.SourceToBlob.Keys;

        public bool TryAddMethod(string address, OscActionPair onReceived)
        {
            if (string.IsNullOrEmpty(address) || onReceived == null) 
                return false;

            switch (OscParser.GetAddressType(address))
            {    
                case AddressType.Address:
                    AddressToMethod.Add(address, onReceived);
                    return true;
                case AddressType.Pattern:
                    int index;
                    // if a method has already been registered for this pattern, add the new delegate
                    if (PatternStringToIndex.TryGetValue(address, out index))
                    {
                        var patternMethod = PatternMethods[index];
                        if (patternMethod is not null)
                        {
                            PatternMethods[index] = patternMethod + onReceived;
                            return true;
                        }
                    }

                    if (FreedPatternIndices.Count > 0)
                    {
                        index = FreedPatternIndices.Dequeue();
                    }
                    else
                    {
                        index = PatternCount;
                        if (index >= Patterns.Length)
                        {
                            var newSize = Patterns.Length * 2;
                            Array.Resize(ref Patterns, newSize);
                            Array.Resize(ref PatternMethods, newSize);
                        }
                    }

                    Patterns[index] = new Regex(address);
                    PatternMethods[index] = onReceived;
                    PatternStringToIndex[address] = index;
                    PatternCount++;
                    return true;
                default: 
                    return false;
            }
        }

        public bool RemoveAddressMethod(string address)
        {
            if (string.IsNullOrEmpty(address))
                return false;

            return OscParser.GetAddressType(address) switch
            {
                AddressType.Address => AddressToMethod.RemoveAddress(address),
                _ => false,
            };
        }

        public bool RemoveMethod(string address, OscActionPair onReceived)
        {
            if (string.IsNullOrEmpty(address) || onReceived == null) 
                return false;

            switch (OscParser.GetAddressType(address))
            {    
                case AddressType.Address:
                    return AddressToMethod.Remove(address, onReceived);
                case AddressType.Pattern:
                    if (!PatternStringToIndex.TryGetValue(address, out var patternIndex))
                        return false;

                    var patternMethod = PatternMethods[patternIndex];
                    if (patternMethod == null)
                        return false;

                    var method = patternMethod.ValueRead;
                    if (method.GetInvocationList().Length == 1)
                    {
                        Patterns[patternIndex] = null;
                        PatternMethods[patternIndex] = null;
                    }
                    else
                    {
                        PatternMethods[patternIndex] = patternMethod - onReceived;
                    }

                    PatternCount--;
                    FreedPatternIndices.Enqueue(patternIndex);
                    return PatternStringToIndex.Remove(address);
                default: 
                    return false;
            }
        }

        /// <summary>
        /// Try to match an address against all known address patterns,
        /// and add a handler for the address if a pattern is matched
        /// </summary>
        /// <param name="address">The address to match</param>
        /// <param name="allMatchedMethods"></param>
        /// <returns>True if a match was found, false otherwise</returns>
        public bool TryMatchPatternHandler(string address, List<OscActionPair> allMatchedMethods)
        {
            if (!OscParser.AddressIsValid(address))
                return false;
            
            allMatchedMethods.Clear();

            bool any = false;
            for (var i = 0; i < PatternCount; i++)
            {
                if (Patterns[i]?.IsMatch(address) ?? false)
                {
                    var handler = PatternMethods[i];
                    if (handler is null)
                        continue;

                    AddressToMethod.Add(address, handler);
                    any = true;
                }
            }

            return any;
        }
    }
}

