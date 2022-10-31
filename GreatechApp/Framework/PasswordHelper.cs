namespace GreatechApp.Framework
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Interactivity;

    /// <summary>
    /// Provide secure method to handle encrypted password using behaviour.
    /// </summary>
    public class PasswordHelper : Behavior<PasswordBox>
    {
        /// <summary>
        /// Property with <see cref="SecureString"/> datatype.
        /// </summary>
        public SecureString Password
        {
            get => (SecureString)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        /// <summary>
        /// Attached property use in xaml
        /// </summary>
        public static DependencyProperty PasswordProperty { get; }
            = DependencyProperty.Register(
                "Password",
                typeof(SecureString),
                typeof(PasswordHelper),
                new PropertyMetadata(null));


        /// <summary>
        /// Attached PasswordChanged event with <see cref="OnPasswordBoxValueChanged(object, RoutedEventArgs)"/>
        /// </summary>
        protected override void OnAttached() => AssociatedObject.PasswordChanged += OnPasswordBoxValueChanged;

        /// <summary>
        /// Event handler for PasswordBox Password property bind PasswordChanged event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPasswordBoxValueChanged(object sender, RoutedEventArgs e)
        {
            BindingExpression binding = BindingOperations.GetBindingExpression(this, PasswordProperty);

            if (binding != null)
            {
                PropertyInfo property = binding.DataItem.GetType().GetProperty(binding.ParentBinding.Path.Path);

                if (property != null)
                {
                    property.SetValue(binding.DataItem, AssociatedObject.SecurePassword, null);
                }
            }
        }

        internal static void GetSecureSTRtoSTR(SecureString password, out string passwordSTR)
        {
            IntPtr passwordBSTR = default(IntPtr);
            string inSecureString = string.Empty;
            try
            {
                passwordBSTR = Marshal.SecureStringToBSTR(password);
                inSecureString = Marshal.PtrToStringBSTR(passwordBSTR);
                passwordSTR = inSecureString;
            }
            catch
            {
                inSecureString = string.Empty;
                passwordSTR = inSecureString;
            }
        }
    }
}
