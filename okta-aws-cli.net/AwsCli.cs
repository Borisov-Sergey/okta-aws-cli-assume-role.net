using System;

namespace okta_aws_cli.net
{
    class AwsCli
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && "logout".Equals(args[0]))
            {
                OktaAwsCliAssumeRole.WithEnvironment(OktaAwsConfig.LoadEnvironment()).LogoutSession();
                Console.WriteLine("You have been logged out");
                return;
            }

            var env = OktaAwsConfig.LoadEnvironment();
            var assumeRole = OktaAwsCliAssumeRole.WithEnvironment(env);
            var profileName = assumeRole.Run(DateTime.Now);

            Console.WriteLine("The role has been successfully assumed. Press any key to exit...");
            Console.ReadLine();

            /*var awsCommand = new List<string>();
            awsCommand.Add("aws");
            awsCommand.Add("--profile");
            awsCommand.Add(profileName);
            awsCommand.AddRange(args);

            ProcessBuilder awsProcessBuilder = new ProcessBuilder().inheritIO().command(awsCommand);
            Process awsSubProcess = awsProcessBuilder.start();

            int exitCode = awsSubProcess.waitFor();
            System.exit(exitCode);*/
        }
    }
}
