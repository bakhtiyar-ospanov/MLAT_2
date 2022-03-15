using System;
using Modules.AppUpdate;
using Modules.AssetMenu;
using Modules.Assets;
using Modules.Awards;
using Modules.Chat;
using Modules.Console.Scripts;
using Modules.Film;
using Modules.FPS;
using Modules.GS_Auth;
using Modules.Help;
using Modules.Intro;
using Modules.Keyboard;
using Modules.LanguageInit;
using Modules.Light;
using Modules.Loading;
using Modules.MainMenu;
using Modules.Multiplayer;
using Modules.News;
using Modules.Notification;
using Modules.Playfab;
using Modules.PostProcessing;
using Modules.S3;
using Modules.Scenario;
using Modules.SpeechKit;
using Modules.Starter;
using Modules.Statistics;
using Modules.VitalSignMonitor;
using Modules.Warning;
using Modules.WorldCourse;
using UnityEngine;

namespace Modules.WDCore
{
    public class GameManager : Singleton<GameManager>
    {
        public Product defaultProduct;
        public ApplicationController applicationController;
        [Header("Modules")]
        public LoadingController loadingController;
        public IntroController introController;
        public AppUpdateController appUpdateController;
        public StarterController starterController;
        public AddressablesS3 addressablesS3;
        public PostProcessController postProcessController;
        public PlayFabLoginController playFabLoginController;
        public PlayFabFileController playFabFileController;
        public PlayFabCurrencyController playFabCurrencyController;
        public PlayFabLeaderboardController playFabLeaderboardController;
        public MainMenuController mainMenuController;
        public SettingsController settingsController;
        public ProfileController profileController;
        public InventoryController inventoryController;
        public FPSController FPSController;
        public AssetMenuController assetMenuController;
        public AssetController assetController;
        public ScenarioSelectorController scenarioSelectorController;
        public ScenarioLoader scenarioLoader;
        public ScenarioController scenarioController;
        public CheckTableController checkTableController;
        public SubtitleController subtitleController;
        public DiseaseHistoryController diseaseHistoryController;
        public DiagnosisSelectorController diagnosisSelectorController;
        public TreatmentSelectorController treatmentSelectorController;
        public LabResultsController labResultsController;
        public LabSelectorController labSelectorController;
        public InstrumentalSelectorController instrumentalSelectorController;
        public ComplaintSelectorController complaintSelectorController;
        public AnamnesisDiseaseSelectorController anamnesisDiseaseSelectorController;
        public AnamnesisLifeSelectorController anamnesisLifeSelectorController;
        public PhysicalExamController physicalExamController;
        public VisualExamController visualExamController;
        public Blackout blackout;
        public StatisticsManager statisticsManager;
        public StatisticsController statisticsController;
        public WCourseSelectorController wCourseSelectorController;
        public WCourseController wCourseController;
        public WarningController warningController;
        public LightController lightController;
        public FilmController filmController;
        public DebriefingController debriefingController;
        public PreviewDownloader previewDownloader;
        public LanguageInitController languageInitController;
        public DebugLogManager debugLogManager;
        public VSMonitorController VSMonitorController;
        public ReportErrorController reportErrorController;
        public GSAuthController GSAuthController;
        public HelpController helpController;
        public AssetRaycastManager assetRaycastManager;
        public AwardController awardController;
        public NotificationController notificationController;
        public NewsController newsController;
        public ClientNetworkManager clientNetworkManager;
        public ChatController chatController;
        
        #if !UNITY_XR
        public AppControls appControls;
        #endif
        
        #if UNITY_XR
        public KeyboardController keyboardController;
        #endif
        
        public bool isBetaTest;
        
        public enum Product
        {
            Vargates,
            Dimedus,
            Academix
        }

        public Action<Product> onProductChange;
    }
}
