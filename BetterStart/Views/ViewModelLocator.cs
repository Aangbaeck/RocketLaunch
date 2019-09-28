/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:RS_Base.Net_2018041.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using BetterStart.Indexing;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using BetterStart.Model;

namespace BetterStart.Views
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<DataService>();
            SimpleIoc.Default.Register<SettingsService>();
            SimpleIoc.Default.Register<MainVM>();
            SimpleIoc.Default.Register<SecondVM>();
            SimpleIoc.Default.Register<IndexingService>();

            ServiceLocator.Current.GetInstance<IndexingService>();
        }

        /// <summary>
        /// Gets the Main property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "This non-static member is needed for data binding purposes.")]
        public MainVM MainVM => ServiceLocator.Current.GetInstance<MainVM>();
        public SecondVM SecondVM => ServiceLocator.Current.GetInstance<SecondVM>();

        /// <summary>
        /// Cleans up all the resources.
        /// </summary>
        public static void Cleanup()
        {
            ServiceLocator.Current.GetInstance<MainVM>().Cleanup();
        }
    }
}