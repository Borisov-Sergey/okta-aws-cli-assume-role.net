using System;
using System.Collections.Generic;

namespace okta_aws_cli.net.Models
{
    public sealed class AccountOption
    {
        public readonly string accountName;
        public readonly List<RoleOption> roleOptions;

        public AccountOption(string accountName, List<RoleOption> roleOptions)
        {
            this.accountName = accountName;
            this.roleOptions = roleOptions;
        }

        public override bool Equals(Object o)
        {
            if (this == o)
                return true;

            if (o == null || this.GetType() != o.GetType())
                return false;

            AccountOption that = (AccountOption)o;
            return object.Equals(accountName, that.accountName) &&
                    object.Equals(roleOptions, that.roleOptions);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int hashingBase = (int)2166136261;
                const int hashingMultiplier = 16777619;

                int hash = hashingBase;
                hash = (hash * hashingMultiplier) ^ (!object.ReferenceEquals(null, accountName) ? accountName.GetHashCode() : 0);
                hash = (hash * hashingMultiplier) ^ (!object.ReferenceEquals(null, roleOptions) ? roleOptions.GetHashCode() : 0);

                return hashingBase;
            }
        }

        public override string ToString()
        {
            return "AccountOption{" +
                    "accountName='" + accountName + '\'' +
                    ", roleOptions=" + roleOptions +
                    '}';
        }
    }
}
