using System;

namespace okta_aws_cli.net.Models
{
    public sealed class RoleOption
    {
        public readonly string roleName;
        public readonly string roleArn;

        public RoleOption(string roleName, string roleArn)
        {
            this.roleName = roleName;
            this.roleArn = roleArn;
        }

        public override bool Equals(Object o)
        {
            if (this == o)
                return true;

            if (o == null || this.GetType() != o.GetType())
                return false;

            RoleOption that = (RoleOption)o;

            return object.Equals(roleName, that.roleName) &&
                    object.Equals(roleArn, that.roleArn);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                const int hashingBase = (int)2166136261;
                const int hashingMultiplier = 16777619;

                int hash = hashingBase;
                hash = (hash * hashingMultiplier) ^ (!object.ReferenceEquals(null, roleName) ? roleName.GetHashCode() : 0);
                hash = (hash * hashingMultiplier) ^ (!object.ReferenceEquals(null, roleArn) ? roleArn.GetHashCode() : 0);

                return hashingBase;
            }
        }

        public override string ToString()
        {
            return "RoleOption{" +
                    "roleName='" + roleName + '\'' +
                    ", roleArn='" + roleArn + '\'' +
                    '}';
        }
    }
}
