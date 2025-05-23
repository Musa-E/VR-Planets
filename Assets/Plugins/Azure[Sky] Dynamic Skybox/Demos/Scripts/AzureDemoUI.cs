using UnityEngine.UI;
using UnityEngine.Profiling;

namespace UnityEngine.AzureSky
{
    public class AzureDemoUI : MonoBehaviour
    {
        [Header("Main References")]
        /// <summary>Reference to the main camera.</summary>
        [SerializeField] private Camera m_camera;
        /// <summary>Reference to the AzureCoreSystem component that controls the time and weather.</summary>
        [SerializeField] private AzureCoreSystem m_azureCoreSystem;
        /// <summary>Reference to the AzureFogScattering component that renders the fog effect to the camera.</summary>
        [SerializeField] private AzureFogScatteringRenderer m_fogScattering;
        /// <summary>Reference to the terrain transform.</summary>
        [SerializeField] private Transform m_terrainTransform;
        /// <summary>Reference to the AzureVolumetricLightRenderer component.</summary>
        [SerializeField] private AzureVolumetricLightRenderer m_volumetricLightRenderer;
        /// <summary>Reference to the AzureFogScatteringRenderer component.</summary>
        [SerializeField] private AzureFogScatteringRenderer m_fogScatteringRenderer;
        /// <summary>Reference to the rain particle system.</summary>
        [SerializeField] private ParticleSystem m_rainParticle;

        [Header("Time UI References")]
        /// <summary>Reference to the day input field.</summary>
        [SerializeField] private InputField m_dayInputField;
        /// <summary>Reference to the day input field.</summary>
        [SerializeField] private InputField m_monthInputField;
        /// <summary>Reference to the day input field.</summary>
        [SerializeField] private InputField m_yearInputField;
        /// <summary>Reference to the day input field.</summary>
        [SerializeField] private InputField m_latitudeInputField;
        /// <summary>Reference to the day input field.</summary>
        [SerializeField] private InputField m_longitudeInputField;
        /// <summary>Reference to the day input field.</summary>
        [SerializeField] private InputField m_utcInputField;

        [Header("Stats UI References")]
        /// <summary>Reference to the fps text.</summary>
        [SerializeField] private Text m_fpsText;
        /// <summary>Reference to the ms text.</summary>
        [SerializeField] private Text m_msText;
        /// <summary>Reference to the reserved memory text.</summary>
        [SerializeField] private Text m_reservedMemoryText;
        /// <summary>Reference to the allocated memory text.</summary>
        [SerializeField] private Text m_allocatedMemoryText;
        /// <summary>Reference to the mono memory text.</summary>
        [SerializeField] private Text m_monoMemoryText;
        /// <summary>Update the fps counter 4 times per second.</summary>
        private int m_updateRate = 4;
        /// <summary>The frame number counter.</summary>
        private int m_frameCount = 0;
        /// <summary>The delta time counter.</summary>
        private float m_deltaTime = 0f;
        /// <summary>The frame rate per second.</summary>
        private float m_fps = 0f;
        /// <summary>The CPU time.</summary>
        private float m_ms = 0f;

        [Header("Weather UI References")]
        /// <summary>Reference to the weather transition bar image.</summary>
        [SerializeField] private Image m_weatherTransitionBar;
        private Vector3 m_scale = Vector3.one;

        private void Start()
        {
            Screen.SetResolution(1920, 1080, true);
        }

        private void OnEnable()
        {
            AzureNotificationCenter.OnWeatherTransitionEnd += OnWeatherTransitionEnd;
        }

        private void OnDisable()
        {
            AzureNotificationCenter.OnWeatherTransitionEnd -= OnWeatherTransitionEnd;
        }

        private void Update()
        {
            // Set memory stats
            m_allocatedMemoryText.text = ((int)(Profiler.GetTotalAllocatedMemoryLong() / 1048576.0f)).ToString() + "MB";
            m_reservedMemoryText.text = ((int)(Profiler.GetTotalReservedMemoryLong() / 1048576.0f)).ToString() + "MB";
            m_monoMemoryText.text = ((int)(Profiler.GetMonoUsedSizeLong() / 1048576.0f)).ToString() + "MB";

            // Set fps stats
            m_deltaTime += Time.unscaledDeltaTime;
            m_frameCount++;

            if (m_deltaTime > 1.0f / m_updateRate)
            {
                m_fps = m_frameCount / m_deltaTime;
                m_ms = m_deltaTime / m_frameCount * 1000.0f;

                m_fpsText.text = Mathf.RoundToInt(m_fps).ToString() + "fps";
                m_msText.text = m_ms.ToString("0.0") + "ms";

                m_deltaTime = 0.0f;
                m_frameCount = 0;
            }

            // Set the weather transition bar
            if (m_azureCoreSystem.weatherSystem.isWeatherChanging)
            {
                m_weatherTransitionBar.transform.localScale = new Vector3(m_azureCoreSystem.weatherSystem.weatherTransitionProgress, 1.0f, 1.0f);
            }


            // Quit the demo application
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        /// <summary>Enables or disables the terrain, according to the toggle state.</summary>
        public void EnableDisableTerrain(Toggle toggle)
        {
            m_terrainTransform.gameObject.SetActive(toggle.isOn);
        }

        /// <summary>Sets the screen resolution, according to the dropdown parameter.</summary>
        public void SetScreenResolution(Dropdown dropdown)
        {
            switch (dropdown.value)
            {
                case 0: // 4K
                    Screen.SetResolution(3840, 2160, true);
                    break;

                case 1: // QUAD HD
                    Screen.SetResolution(2560, 1440, true);
                    break;

                case 2: // FULL HD
                    Screen.SetResolution(1920, 1080, true);
                    break;

                case 3: // HD
                    Screen.SetResolution(1280, 720, true);
                    break;
            }
        }

        /// <summary>Sets the resolution of the fog scattering texture, according to the dropdown parameter.</summary>
        public void SetFogScatteringResolution(Dropdown dropdown)
        {
            switch (dropdown.value)
            {
                case 0: // 4K
                    m_fogScattering.SetFogScatteringResolution(3840, 2160);
                    break;

                case 1: // QUAD HD
                    m_fogScattering.SetFogScatteringResolution(2560, 1440);
                    break;

                case 2: // FULL HD
                    m_fogScattering.SetFogScatteringResolution(1920, 1080);
                    break;

                case 3: // HD
                    m_fogScattering.SetFogScatteringResolution(1280, 720);
                    break;

                case 4: // qFull HD
                    m_fogScattering.SetFogScatteringResolution(960, 540);
                    break;

                case 5: // qHD
                    m_fogScattering.SetFogScatteringResolution(640, 360);
                    break;
            }
        }

        /// <summary>Sets the time of day mode to simple or realistic, according to the dropdown parameter.</summary>
        public void SetTimeMode(Dropdown dropdown)
        {
            switch (dropdown.value)
            {
                case 0: // Simple
                    m_dayInputField.transform.parent.gameObject.SetActive(false);
                    m_monthInputField.transform.parent.gameObject.SetActive(false);
                    m_yearInputField.transform.parent.gameObject.SetActive(false);
                    m_latitudeInputField.transform.parent.gameObject.SetActive(false);
                    m_longitudeInputField.transform.parent.gameObject.SetActive(false);
                    m_utcInputField.transform.parent.gameObject.SetActive(false);

                    m_azureCoreSystem.timeSystem.timeMode = AzureTimeMode.Simple;
                    m_azureCoreSystem.timeSystem.latitude = 0.0f;
                    m_azureCoreSystem.timeSystem.longitude = 0.0f;
                    m_azureCoreSystem.timeSystem.utc = 0.0f;
                    m_azureCoreSystem.timeSystem.SetDate(1, 1, 2024);
                    break;

                case 1: // Realistic
                    m_dayInputField.transform.parent.gameObject.SetActive(true);
                    m_monthInputField.transform.parent.gameObject.SetActive(true);
                    m_yearInputField.transform.parent.gameObject.SetActive(true);
                    m_latitudeInputField.transform.parent.gameObject.SetActive(true);
                    m_longitudeInputField.transform.parent.gameObject.SetActive(true);
                    m_utcInputField.transform.parent.gameObject.SetActive(true);

                    m_azureCoreSystem.timeSystem.timeMode = AzureTimeMode.Realistic;
                    m_azureCoreSystem.timeSystem.latitude = int.Parse(m_latitudeInputField.text);
                    m_azureCoreSystem.timeSystem.longitude = int.Parse(m_longitudeInputField.text);
                    m_azureCoreSystem.timeSystem.utc = int.Parse(m_utcInputField.text);
                    m_azureCoreSystem.timeSystem.SetDate(int.Parse(m_dayInputField.text), int.Parse(m_monthInputField.text), int.Parse(m_yearInputField.text));
                    break;
            }
        }

        /// <summary>Update the time system when there is a change in the time UI elements.</summary>
        public void UpdateTimeSettings()
        {
            m_azureCoreSystem.timeSystem.latitude = int.Parse(m_latitudeInputField.text);
            m_azureCoreSystem.timeSystem.longitude = int.Parse(m_longitudeInputField.text);
            m_azureCoreSystem.timeSystem.utc = int.Parse(m_utcInputField.text);
            m_azureCoreSystem.timeSystem.SetDate(int.Parse(m_dayInputField.text), int.Parse(m_monthInputField.text), int.Parse(m_yearInputField.text));
        }

        /// <summary>Quit the application from a UI button click.</summary>
        public void ApplicationQuit()
        {
            Application.Quit();
        }

        /// <summary>Register or Unregister the 'UpdateDynamicGI' to the core update event, according to the parameter toggle.</summary>
        public void OnUpdateDynamicGIToggleChanged(Toggle toggle)
        {
            switch (toggle.isOn)
            {
                case true:
                    //m_azureCoreSystem.onCoreUpdateEvent.AddListener(m_azureCoreSystem.UpdateDynamicGI);
                    //UnityEventTools.AddPersistentListener(m_azureCoreSystem.onCoreUpdateEvent, m_azureCoreSystem.UpdateDynamicGI);
                    //UnityEventTools.RegisterPersistentListener(m_azureCoreSystem.onCoreUpdateEvent, 1, m_azureCoreSystem.UpdateDynamicGI);
                    m_azureCoreSystem.onCoreUpdateEvent.SetPersistentListenerState(1, Events.UnityEventCallState.RuntimeOnly);
                    break;

                case false:
                    //m_azureCoreSystem.onCoreUpdateEvent.RemoveListener(m_azureCoreSystem.UpdateDynamicGI);
                    //UnityEventTools.RemovePersistentListener(m_azureCoreSystem.onCoreUpdateEvent, m_azureCoreSystem.UpdateDynamicGI);
                    //UnityEventTools.UnregisterPersistentListener(m_azureCoreSystem.onCoreUpdateEvent, 1);
                    m_azureCoreSystem.onCoreUpdateEvent.SetPersistentListenerState(1, Events.UnityEventCallState.Off);
                    break;
            }
        }

        /// <summary>Register or Unregister the 'UpdateReflectionProbe' to the core update event, according to the parameter toggle.</summary>
        public void OnUpdateReflectionProbeToggleChanged(Toggle toggle)
        {
            switch (toggle.isOn)
            {
                case true:
                    //m_azureCoreSystem.onCoreUpdateEvent.AddListener(m_azureCoreSystem.UpdateReflectionProbe);
                    //UnityEventTools.AddPersistentListener(m_azureCoreSystem.onCoreUpdateEvent, m_azureCoreSystem.UpdateReflectionProbe);
                    //UnityEventTools.RegisterPersistentListener(m_azureCoreSystem.onCoreUpdateEvent, 2, m_azureCoreSystem.UpdateReflectionProbe);
                    m_azureCoreSystem.onCoreUpdateEvent.SetPersistentListenerState(2, Events.UnityEventCallState.EditorAndRuntime);
                    break;

                case false:
                    //m_azureCoreSystem.onCoreUpdateEvent.RemoveListener(m_azureCoreSystem.UpdateReflectionProbe);
                    //UnityEventTools.RemovePersistentListener(m_azureCoreSystem.onCoreUpdateEvent, m_azureCoreSystem.UpdateReflectionProbe);
                    //UnityEventTools.UnregisterPersistentListener(m_azureCoreSystem.onCoreUpdateEvent, 2);
                    m_azureCoreSystem.onCoreUpdateEvent.SetPersistentListenerState(2, Events.UnityEventCallState.Off);
                    break;
            }
        }

        /// <summary>Enables or Disables the VSync, according to the toggle passed as parameter.</summary>
        public void OnVSyncToggleChanged(Toggle toggle)
        {
            switch (toggle.isOn)
            {
                case true:
                    QualitySettings.vSyncCount = 1;
                    break;

                case false:
                    QualitySettings.vSyncCount = 0;
                    break;
            }
        }

        /// <summary>Enables or disables the volumetric lights rendering, according to the toggle state.</summary>
        public void EnableDisableVolumetricLights(Toggle toggle)
        {
            m_volumetricLightRenderer.enabled = toggle.isOn;
        }

        /// <summary>Enables or disables the fog scattering rendering, according to the toggle state.</summary>
        public void EnableDisableFogScattering(Toggle toggle)
        {
            m_fogScatteringRenderer.enabled = toggle.isOn;
        }

        /// <summary>Enables or disables the particles sub emitters nodule, according to the toggle state.</summary>
        public void EnableDisableParticleSubEmitters(Toggle toggle)
        {
            var sub = m_rainParticle.subEmitters;
            sub.enabled = toggle.isOn;
        }

        /// <summary>Set the global weather from a button click event.</summary>
        public void SetGlobalWeather(int index)
        {
            m_azureCoreSystem.weatherSystem.SetGlobalWeather(index);
        }

        /// <summary>Instantiate the first thunder index from the thunder list using a button click event.</summary>
        public void InstantiateThunder()
        {
            m_azureCoreSystem.weatherSystem.InstantiateThunderPrefab(0);
            //m_azureCoreSystem.weatherSystem.InstantiateThunderPrefab(0, new Vector3(0.0f, 600.0f, 1250.0f));
        }

        /// <summary>Set the timeline using a slider as parameter.</summary>
        public void SetTimeline(Slider slider)
        {
            m_azureCoreSystem.timeSystem.timeline = slider.value;
        }

        /// <summary>Set the camera altitude using a slider as parameter.</summary>
        public void SetCameraAltitude(Slider slider)
        {
            m_camera.transform.position = new Vector3(0.0f, slider.value, 0.0f);
        }

        /// <summary>Event triggered when a weather transition ends.</summary>
        private void OnWeatherTransitionEnd(AzureWeatherSystem azureWeatherSystem)
        {
            m_weatherTransitionBar.transform.localScale = new Vector3(0.0f, 1.0f, 1.0f);
        }
    }
}