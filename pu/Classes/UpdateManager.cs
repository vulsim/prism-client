using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Deployment.Application;
using System.Windows;
using System.ComponentModel;

namespace Prism.Classes
{
    public class UpdateManager
    {
        public delegate void UpdateManagerProgressEventHandler(object sender, string stage);

        public static UpdateManager Instance = null;

        public event UpdateManagerProgressEventHandler UpdateManagerProgressEvent;
        private System.Timers.Timer UpdateTimer;
        private ApplicationDeployment Delpoyment;

        public UpdateManager()
        {
            UpdateTimer = new System.Timers.Timer(new TimeSpan(2, 0, 0).TotalMilliseconds);
            UpdateTimer.Elapsed += UpdateTimerEvent;
            //UpdateTimer.Start();
        }

        ~UpdateManager()
        {
            UpdateTimer.Stop();
            UpdateTimer.Elapsed -= UpdateTimerEvent;
        }

        public void CheckForUpdate()
        {
            if (Delpoyment != null)
            {
                return;
            }

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                UpdateTimer.Stop();

                if (UpdateManagerProgressEvent != null)
                {
                    UpdateManagerProgressEvent(this, "Проверка обновлений...");
                }

                try
                {
                    Delpoyment = ApplicationDeployment.CurrentDeployment;
                    Delpoyment.CheckForUpdateCompleted += CheckForUpdateCompleted;
                    Delpoyment.CheckForUpdateProgressChanged += CheckForUpdateProgressChanged;
                    Delpoyment.CheckForUpdateAsync();
                }
                catch (Exception e)
                {
                    if (UpdateManagerProgressEvent != null)
                    {
                        UpdateManagerProgressEvent(this, "");
                    }
                }                
            }
        }

        private void BeginUpdate()
        {
            try
            {
                Delpoyment.UpdateCompleted += UpdateCompleted;
                Delpoyment.UpdateProgressChanged += UpdateProgressChanged;
                Delpoyment.UpdateAsync();
            }
            catch (Exception e)
            {
                if (UpdateManagerProgressEvent != null)
                {
                    UpdateManagerProgressEvent(this, "");
                }
            }             
        }

        private void UpdateTimerEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            //CheckForUpdate();
        }

        private void CheckForUpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            if (UpdateManagerProgressEvent != null)
            {
                UpdateManagerProgressEvent(this, String.Format("Получение {0}... ({1:D}K из {2:D}K загружено)", GetProgressString(e.State), e.BytesCompleted / 1024, e.BytesTotal / 1024));
            }
        }

        private void UpdateProgressChanged(object sender, DeploymentProgressChangedEventArgs e)
        {
            if (UpdateManagerProgressEvent != null)
            {
                UpdateManagerProgressEvent(this, String.Format("Обновление... ({0}%)", e.ProgressPercentage));
            }
        }

        private void CheckForUpdateCompleted(object sender, CheckForUpdateCompletedEventArgs e)
        {
            try
            {
                Delpoyment.CheckForUpdateCompleted -= CheckForUpdateCompleted;
                Delpoyment.CheckForUpdateProgressChanged -= CheckForUpdateProgressChanged;

                if (UpdateManagerProgressEvent != null)
                {
                    UpdateManagerProgressEvent(this, "");
                }

                if (e.UpdateAvailable)
                {
                    if (!e.IsUpdateRequired)
                    {
                        if (MessageBox.Show("Обнаружено, что установленная версия программы является устаревшей. Выполнить обновление?", "Требуется обновление", MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                        {
                            BeginUpdate();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Доступно обязательное обновоение для вашего приложения. Обновление будет выполнено автоматически, после чего приложение необходимо будет перезагрузить.", "Требуется обновление", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);
                        BeginUpdate();
                    }
                }
                else
                {
                    Delpoyment = null;
                    //UpdateTimer.Start();
                }                
            }
            catch (Exception exception)
            {
                if (UpdateManagerProgressEvent != null)
                {
                    UpdateManagerProgressEvent(this, "");
                }
            }            
        }
        
        private void UpdateCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                Delpoyment.UpdateCompleted -= UpdateCompleted;
                Delpoyment.UpdateProgressChanged -= UpdateProgressChanged;

                if (UpdateManagerProgressEvent != null)
                {
                    UpdateManagerProgressEvent(this, "");
                }

                if (e.Error != null || e.Cancelled)
                {
                    Delpoyment = null;
                    //UpdateTimer.Start();
                    return;
                }

                if (MessageBox.Show("Приложение было обновлено, необходимо перезагрузить приложение для вступления изменений в силу. Выполнить перезагрузку сейчас?", "Перезагрука приложения", MessageBoxButton.OKCancel, MessageBoxImage.Information, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    Core.Instance.Shutdown();
                }
                else
                {
                    Delpoyment = null;
                    //UpdateTimer.Start();
                }
            }
            catch (Exception exception)
            {
                if (UpdateManagerProgressEvent != null)
                {
                    UpdateManagerProgressEvent(this, "");
                }
            }
        }

        private string GetProgressString(DeploymentProgressState state)
        {
            if (state == DeploymentProgressState.DownloadingApplicationFiles)
            {
                return "файлов приложения";
            }
            else if (state == DeploymentProgressState.DownloadingApplicationInformation)
            {
                return "манифеста приложения";
            }
            else
            {
                return "манифеста распространения";
            }
        }
    }
}
