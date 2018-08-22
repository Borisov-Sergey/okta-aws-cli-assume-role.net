using System;

using log4net;

namespace okta_aws_cli.net.Helpers
{
    public sealed class MenuHelper
    {
        private static readonly ILog logger = LogManager.GetLogger(nameof(MenuHelper));

        /**
         * Prompt the user to select an option from a menu of options
         *
         * @param max The maximum number of options
         * @return The selected option
         */
        public static int PromptForMenuSelection(int max)
        {
            if (max == 1)
                return 0;

            int selection = -1;
            while (selection == -1)
            {
                //prompt user for selection
                Console.WriteLine("Selection: ");
                String selectInput = Console.ReadLine();

                try
                {
                    selection = int.Parse(selectInput) - 1;
                    if (selection < 0 || selection >= max)
                    {
                        throw new ArgumentException();
                    }
                }
                catch (ArgumentException e)
                {
                    logger.Error("Invalid input: Please enter a valid selection\n");
                    selection = -1;
                }
                catch (FormatException e)
                {
                    logger.Error("Invalid input: Please enter in a number \n");
                    selection = -1;
                }
            }

            return selection;
        }
    }
}
