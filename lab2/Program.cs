namespace lab2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string cmd = args[0];
            string inputFileName = args[1];
            string outputFileName = args[2];

            switch (cmd)
            {
                case "mealy":
                    {
                        new MinMealy(inputFileName, outputFileName).Minimize();
                        break;
                    }
                case "moore":
                    {
                        new MinMoore(inputFileName, outputFileName).Minimize();
                        break;
                    }
            }
        }
    }
}
