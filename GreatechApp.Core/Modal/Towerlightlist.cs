using GreatechApp.Core.Enums;
using Prism.Mvvm;

namespace GreatechApp.Core.Modal
{
    public class Towerlightlist : BindableBase
    {
        public Towerlightlist()
        {
            RedState = TowerLightState.OFF;
            YellowState = TowerLightState.OFF;
            GreenState = TowerLightState.OFF;
            BuzzerState = TowerLightState.OFF;
        }

        public string Name { get; set; }

        public MachineStateType Tag { get; set; }

        private TowerLightState m_redstate;
        public TowerLightState RedState
        {
            get { return m_redstate; }
            set
            {
                SetProperty(ref m_redstate, value);
            }
        }

        private TowerLightState m_yellowstate;
        public TowerLightState YellowState
        {
            get { return m_yellowstate; }
            set
            {
                SetProperty(ref m_yellowstate, value);
            }
        }

        private TowerLightState m_greenstate;
        public TowerLightState GreenState
        {
            get { return m_greenstate; }
            set
            {
                SetProperty(ref m_greenstate, value);
            }
        }

        private TowerLightState m_buzzerstate;
        public TowerLightState BuzzerState
        {
            get { return m_buzzerstate; }
            set
            {
                SetProperty(ref m_buzzerstate, value);
            }
        }

        #region Red Light
        private bool m_RSteady;
        public bool RSteady
        {
            get { return m_RSteady; }
            set
            {
                SetProperty(ref m_RSteady, value);
                if (value)
                    RedState = TowerLightState.ON;
            }
        }

        private bool m_RBlinking;
        public bool RBlinking
        {
            get { return m_RBlinking; }
            set 
            { 
                SetProperty(ref m_RBlinking, value); 
                if (value)
                    RedState = TowerLightState.Blinking;
            }
        }

        private bool m_ROff;
        public bool ROff
        {
            get { return m_ROff; }
            set
            {
                SetProperty(ref m_ROff, value);
                if (value)
                    RedState = TowerLightState.OFF;
            }
        }
        #endregion

        #region Yellow Light
        private bool m_YSteady;
        public bool YSteady
        {
            get { return m_YSteady; }
            set 
            { 
                SetProperty(ref m_YSteady, value);
                if (value)
                    YellowState = TowerLightState.ON;
            }
        }

        private bool m_YBlinking;
        public bool YBlinking
        {
            get { return m_YBlinking; }
            set 
            { 
                SetProperty(ref m_YBlinking, value);
                if (value)
                    YellowState = TowerLightState.Blinking;
            }
        }

        private bool m_YOff;
        public bool YOff
        {
            get { return m_YOff; }
            set 
            { 
                SetProperty(ref m_YOff, value);
                if (value)
                    YellowState = TowerLightState.OFF;
            }
        }
        #endregion

        #region Green Light
        private bool m_GSteady;
        public bool GSteady
        {
            get { return m_GSteady; }
            set 
            { 
                SetProperty(ref m_GSteady, value);
                if (value)
                    GreenState = TowerLightState.ON;
            }
        }

        private bool m_GBlinking;
        public bool GBlinking
        {
            get { return m_GBlinking; }
            set 
            { 
                SetProperty(ref m_GBlinking, value);
                if (value)
                    GreenState = TowerLightState.Blinking;
            }
        }

        private bool m_GOff;
        public bool GOff
        {
            get { return m_GOff; }
            set 
            { 
                SetProperty(ref m_GOff, value);
                if (value)
                    GreenState = TowerLightState.OFF;
            }
        }
        #endregion

        #region Buzzer
        private bool m_BuzBlinking;
        public bool BuzBlinking
        {
            get { return m_BuzBlinking; }
            set 
            { 
                SetProperty(ref m_BuzBlinking, value);
                if (value)
                    BuzzerState = TowerLightState.Blinking;
            }
        }

        private bool m_BuzOff;
        public bool BuzOff
        {
            get { return m_BuzOff; }
            set 
            { 
                SetProperty(ref m_BuzOff, value);
                if (value)
                    BuzzerState = TowerLightState.OFF;
            }
        }
        #endregion
    }
}
