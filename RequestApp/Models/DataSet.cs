using System;

namespace RequestApp.Models
{
    public class ITCDataSet
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string ProjectName => GetPrefix(Name);

        public string GetPrefix(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            int numberCount = 0;
            bool firstCharIsNumber = char.IsDigit(input[0]);

            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]))
                {
                    numberCount++;

                    // If first char is NOT a number:
                    // return everything before the first number
                    if (!firstCharIsNumber)
                    {
                        return input[..i];
                    }

                    // If first char IS a number:
                    // return everything before the second number
                    if (firstCharIsNumber && numberCount == 2)
                    {
                        return input[..i];
                    }
                }
            }

            return input;
        }

        public ITCDataSet Clone()
        {
            return new ITCDataSet
            {
                ID = ID,
                Name = Name
                
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
