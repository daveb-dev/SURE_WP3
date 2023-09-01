/**
	Sustainable Energy Development game modeling the Swiss energy Grid.
	Copyright (C) 2023 Università della Svizzera Italiana

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using Godot;
using System;
using System.Diagnostics;

// General controller for the UI
public partial class UI : CanvasLayer {

	// Describes the type of bar that contains information about certain metrics
	public enum InfoType { W_ENGERGY, S_ENGERGY, SUPPORT, ENVIRONMENT, POLLUTION, MONEY };

	// XML querying strings
	private const string LABEL_FILENAME = "labels.xml";
	private const string INFOBAR_GROUP = "infobar";
	private const string RES_GROUP = "resources";
	private const string POWERPLANT_GROUP = "powerplants";	
	private const string UI_GROUP = "ui";

	// Signals to the game loop that the turn must be passed
	[Signal]
	public delegate void NextTurnEventHandler();

	// Timeline update values
	[Export]
	public int TIMELINE_STEP_SIZE = 3;
	[Export]
	public int TIMELINE_MAX_VALUE = 2050;


	// Contains the data displayed in the UI
	private InfoData Data;

	// TextController reference set by the game loop
	private TextController TC;

	// Button that triggers the passage to a next turn
	private TextureButton NextTurnButton;
	private Label NextTurnL;

	// The two energy bars, showing the availability and demand
	private InfoBar WinterEnergy;
	private InfoBar WinterEnergyPredict;
	private InfoBar SummerEnergy;
	private InfoBar SummerEnergyPredict;
	
	// The two information bars
	private InfoBar EnvironmentBar;
	private InfoBar SupportBar;
	private InfoBar PollutionBar;

	// Date progression
	private HSlider Timeline;
	private AnimationPlayer TimelineAP;

	// Imports
	private ImportSlider Imports;
	

	// Money related nodes
	private Label MoneyL;
	private Label MoneyNameL;
	private Label BudgetNameL;
	private Label BuildNameL;
	private Label ProdNameL;
	private Label ImportCostNameL;
	private Button MoneyButton;
	private ColorRect MoneyInfo;
	private Label BudgetL;
	private Label BuildL;
	private Label ProdL;
	private Label ImportCostL;

	// Window buttons
	private TextureButton PolicyButton;
	private TextureButton StatsButton;

	// Windows
	private PolicyWindow PW;

	// Build Menu
	private BuildMenu BM;

	// Settings
	private Button SettingsButton;
	private ColorRect SettingsBox;
	private Button LanguageButton;
	private Button SettingsClose;

	// Game Loop
	private GameLoop GL;

	// Context
	private Context C;

	// ==================== GODOT Method Overrides ====================

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		// Fetch Nodes
		NextTurnButton = GetNode<TextureButton>("NextTurn");
		NextTurnL = GetNode<Label>("NextTurn/Label");
		TC = GetNode<TextController>("../TextController");
		BM = GetNode<BuildMenu>("../BuildMenu");
		GL = GetOwner<GameLoop>();
		C = GetNode<Context>("/root/Context");

		// Settings
		SettingsButton = GetNode<Button>("SettingsButton");
		SettingsBox = GetNode<ColorRect>("SettingsButton/SettingsBox");
		LanguageButton = GetNode<Button>("SettingsButton/SettingsBox/VBoxContainer/Language");
		SettingsClose = GetNode<Button>("SettingsButton/SettingsBox/Close");

		// Info Bars
		WinterEnergy = GetNode<InfoBar>("EnergyBarWinter");
		WinterEnergyPredict = GetNode<InfoBar>("EnergyBarWinterPredict");
		SummerEnergy = GetNode<InfoBar>("EnergyBarSummer");
		SummerEnergyPredict = GetNode<InfoBar>("EnergyBarSummerPredict");
		EnvironmentBar = GetNode<InfoBar>("Env");
		SupportBar = GetNode<InfoBar>("Trust");
		PollutionBar = GetNode<InfoBar>("Poll");

		// Sliders
		Timeline = GetNode<HSlider>("Top/Timeline");
		Imports = GetNode<ImportSlider>("Import");
		
		TimelineAP = GetNode<AnimationPlayer>("TimePanelBlank/TimelineAnimation");

		// Money Nodes
		MoneyL = GetNode<Label>("Money/money");
		MoneyButton = GetNode<Button>("MoneyUI");
		MoneyInfo = GetNode<ColorRect>("MoneyInfo");
		BudgetL = GetNode<Label>("MoneyInfo/budget");
		BuildL = GetNode<Label>("MoneyInfo/build");
		ProdL = GetNode<Label>("MoneyInfo/prod");
		ImportCostL = GetNode<Label>("MoneyInfo/importamounts");

		// Name labels
		MoneyNameL = GetNode<Label>("Money/Label");
		BudgetNameL = GetNode<Label>("MoneyInfo/VBoxContainer/Label3");
		BuildNameL = GetNode<Label>("MoneyInfo/VBoxContainer/Label4");
		ProdNameL = GetNode<Label>("MoneyInfo/VBoxContainer/Label2");
		ImportCostNameL = GetNode<Label>("MoneyInfo/VBoxContainer/Import");

		// Window buttons
		PolicyButton = GetNode<TextureButton>("PolicyButton");
		StatsButton = GetNode<TextureButton>("Stats");

		// Windows
		PW = GetNode<PolicyWindow>("Window");

		// Connect Various signals
		MoneyButton.Pressed += _OnMoneyButtonPressed;
		NextTurnButton.Pressed += _OnNextTurnPressed;
		SettingsButton.Pressed += _OnSettingsButtonPressed;
		LanguageButton.Pressed += _OnLanguageButtonPressed;
		SettingsClose.Pressed += _OnSettingsClosePressed;
		PolicyButton.Pressed += _OnPolicyButtonPressed;
		WinterEnergy.MouseEntered += _OnWinterEnergyMouseEntered;
		WinterEnergy.MouseExited += _OnWinterEnergyMouseExited;
		WinterEnergyPredict.MouseEntered += _OnWinterEnergyMouseEntered;
		WinterEnergyPredict.MouseExited += _OnWinterEnergyMouseExited;
		SummerEnergy.MouseEntered += _OnSummerEnergyMouseEntered;
		SummerEnergy.MouseExited += _OnSummerEnergyMouseExited;
		SummerEnergyPredict.MouseEntered += _OnSummerEnergyMouseEntered;
		SummerEnergyPredict.MouseExited += _OnSummerEnergyMouseExited;
		EnvironmentBar.MouseEntered += _OnEnvironmentMouseEntered;
		EnvironmentBar.MouseExited += _OnEnvironmentMouseExited;
		SupportBar.MouseEntered += _OnSupportMouseEntered;
		SupportBar.MouseExited += _OnSupportMouseExited;
		PollutionBar.MouseEntered += _OnPollutionMouseEntered;
		PollutionBar.MouseExited += _OnPollutionMouseExited;
		Imports.ImportUpdate += _OnImportUpdate;

		// For predictive updates
		C.UpdatePrediction += _OnUpdatePrediction;
		TC.UpdateUI += _UpdateUI;

		// Initialize data
		Data = new InfoData();

		// Set the language
		C._UpdateLanguage(Language.Type.EN);
	}

	// ==================== UI Update API ====================

	// Updates the various labels across the UI
	public void _UpdateUI() {
		// Updates the displayed language to match the selected one
		LanguageButton.Text = C._GetLanguageName();

		// Fetch the build menu names
		string gas_name = TC._GetText(LABEL_FILENAME, POWERPLANT_GROUP, "label_gas");
		string hydro_name = TC._GetText(LABEL_FILENAME, POWERPLANT_GROUP, "label_hydro");
		string solar_name = TC._GetText(LABEL_FILENAME, POWERPLANT_GROUP, "label_solar");
		string wind_name = TC._GetText(LABEL_FILENAME, POWERPLANT_GROUP, "label_wind");
		string tree_name = TC._GetText(LABEL_FILENAME, POWERPLANT_GROUP, "label_tree");
		string nuclear_name = TC._GetText(LABEL_FILENAME, POWERPLANT_GROUP, "label_nuclear");
		string waste_name = TC._GetText(LABEL_FILENAME, POWERPLANT_GROUP, "label_waste");
		string biomass_name = TC._GetText(LABEL_FILENAME, POWERPLANT_GROUP, "label_biomass");
		string river_name = TC._GetText(LABEL_FILENAME, POWERPLANT_GROUP, "label_river");
		string pump_name = TC._GetText(LABEL_FILENAME, POWERPLANT_GROUP, "label_pump");

		// Fetch the energy bar names
		string WinterEnergy_name = TC._GetText(LABEL_FILENAME, RES_GROUP, "label_energy_w");
		string SummerEnergy_name = TC._GetText(LABEL_FILENAME, RES_GROUP, "label_energy_s");
		string EnvironmentBar_name = TC._GetText(LABEL_FILENAME, RES_GROUP, "label_environment");
		string SupportBar_name = TC._GetText(LABEL_FILENAME, RES_GROUP, "label_support");
		string PollutionBar_name = TC._GetText(LABEL_FILENAME, RES_GROUP, "label_pollution");

		// UI buttons
		string next_turn_name = TC._GetText(LABEL_FILENAME, UI_GROUP, "label_next_turn");
		string import_name = TC._GetText(LABEL_FILENAME, UI_GROUP, "label_import");

		// Update the various plants
		BM._UpdatePlantName(Building.Type.GAS, gas_name);
		BM._UpdatePlantName(Building.Type.HYDRO, hydro_name);
		BM._UpdatePlantName(Building.Type.SOLAR, solar_name);
		BM._UpdatePlantName(Building.Type.TREE, tree_name);
		BM._UpdatePlantName(Building.Type.WIND, wind_name);
		BM._UpdatePlantName(Building.Type.WASTE, waste_name);
		BM._UpdatePlantName(Building.Type.BIOMASS, biomass_name);
		BM._UpdatePlantName(Building.Type.RIVER, river_name);
		BM._UpdatePlantName(Building.Type.PUMP, pump_name);

		// Update the placed plants
		foreach(PowerPlant pp in GL._GetPowerPlants()) {
			switch(pp.PlantType.type) {
				case Building.Type.GAS:
					pp._UpdatePlantName(gas_name);
					break;
				case Building.Type.HYDRO:
					pp._UpdatePlantName(hydro_name);
					break;
				case Building.Type.SOLAR:
					pp._UpdatePlantName(solar_name);
					break;
				case Building.Type.TREE:
					pp._UpdatePlantName(tree_name);
					break;
				case Building.Type.NUCLEAR:
					pp._UpdatePlantName(nuclear_name);
					break;
				case Building.Type.WIND:
					pp._UpdatePlantName(wind_name);
					break;
				case Building.Type.WASTE:
					pp._UpdatePlantName(waste_name);
					break;
				case Building.Type.BIOMASS:
					pp._UpdatePlantName(biomass_name);
					break;
				case Building.Type.RIVER:
					pp._UpdatePlantName(river_name);
					break;
				case Building.Type.PUMP:
					pp._UpdatePlantName(pump_name);
					break;	
				default:
					break;
			}
		}

		// Update the resource bar names
		WinterEnergy._UpdateBarName(WinterEnergy_name);
		SummerEnergy._UpdateBarName(SummerEnergy_name);
		EnvironmentBar._UpdateBarName(EnvironmentBar_name);
		SupportBar._UpdateBarName(SupportBar_name);
		PollutionBar._UpdateBarName(PollutionBar_name);

		// Update UI buttons
		NextTurnL.Text = next_turn_name;

		// Update the import slider
		Imports._UpdateLabel(import_name);

		// Upate the money labels
		SetMoneyInfo();
	}

	// Updates the value of the a given bar
	public void _UpdateBarValue(InfoType t, int val) {
		switch (t) {
			case InfoType.W_ENGERGY:
				WinterEnergy._UpdateProgress(val);
				break;
			case InfoType.S_ENGERGY:
				SummerEnergy._UpdateProgress(val);
				break;
			case InfoType.SUPPORT:
				SupportBar._UpdateProgress(val);
				break;
			case InfoType.ENVIRONMENT:
				EnvironmentBar._UpdateProgress(val);
				break;
			case InfoType.POLLUTION:
				PollutionBar._UpdateProgress(val);
				break;
			default:
				break;
		}
	}

	// Updates the slider in the middle of the a given bar
	public void _UpdateBarSlider(InfoType t, float val) {
		switch (t) {
			// For the energy sliders, the value represents the percentage of the total allowed capacity
			// that is allowed on the grid that the demand takes up
			case InfoType.W_ENGERGY:
				WinterEnergy._UpdateSlider(val);
				WinterEnergyPredict._UpdateSlider(val);
				break;
			case InfoType.S_ENGERGY:
				SummerEnergy._UpdateSlider(val);
				SummerEnergyPredict._UpdateSlider(val);
				break;
			case InfoType.SUPPORT:
				SupportBar._UpdateSlider(val);
				break;
			case InfoType.ENVIRONMENT:
				EnvironmentBar._UpdateSlider(val);
				break;
			case InfoType.POLLUTION:
				PollutionBar._UpdateSlider(val);
				break;
			default:
				break;
		}
	}

	// Update the internal UI data
	// This is done following the ordering of the fields in the InfoData struct
	// Energy: demand, supply
	// Support: energy_affordability, env_aesthetic
	// Environment: land_use, pollution, biodiversity, envbarval, import pollution
	// Money: budget, production, building, money, import cost
	public void _UpdateData(InfoType t, params int[] d) {
		switch (t) {
			case InfoType.W_ENGERGY:
				// Sanity check, make sure that you were given enough fields
				Debug.Assert(d.Length >= InfoData.N_W_ENERGY_FIELDS);

				// Set the fields in order, for energy it's demand, supply
				Data.W_EnergyDemand = d[0];
				Data.W_EnergySupply = d[1];

				// Set the text fields
				SetEnergyInfo(ref WinterEnergy, InfoType.W_ENGERGY);

				// Update the bar value
				_UpdateBarValue(InfoType.W_ENGERGY, Data.W_EnergySupply);

				// Update the bar slider
				_UpdateBarSlider(
					InfoType.W_ENGERGY, 
					(float)Data.W_EnergyDemand / EnergyManager.MAX_ENERGY_BAR_VAL
                );
				
				WinterEnergy._UpdateColor(Data.W_EnergySupply < Data.W_EnergyDemand);

				// Update the required import target (only in winter due to conservative estimates)
				SetTargetImport();
				break;
			case InfoType.S_ENGERGY:
				// Sanity check, make sure that you were given enough fields
				Debug.Assert(d.Length >= InfoData.N_S_ENERGY_FIELDS);

				// Set the fields in order, for energy it's demand, supply
				Data.S_EnergyDemand = d[0];
				Data.S_EnergySupply = d[1];

				// Update the UI
				SetEnergyInfo(ref SummerEnergy, InfoType.S_ENGERGY);

				// Update the bar value
				_UpdateBarValue(InfoType.S_ENGERGY, Data.S_EnergySupply);

				// Update the bar slider
				_UpdateBarSlider(
					InfoType.S_ENGERGY, 
					(float)Data.S_EnergyDemand / (float)EnergyManager.MAX_ENERGY_BAR_VAL
				);
				
				SummerEnergy._UpdateColor(Data.S_EnergySupply < Data.S_EnergyDemand);
				break;
			case InfoType.SUPPORT:
				// Sanity check, make sure that you were given enough fields
				Debug.Assert(d.Length >= InfoData.N_SUPPORT_FIELDS);

				// Set the fields in order, for support it's affordability, aesthetic
				Data.EnergyAffordability = d[0];
				Data.EnvAesthetic = d[1];

				// Update the UI
				SetSupportInfo();
				break;
			case InfoType.ENVIRONMENT:
				// Sanity check, make sure that you were given enough fields
				Debug.Assert(d.Length >= InfoData.N_ENV_FIELDS);

				// Set the fields in order, for environment it's landuse, pollution, biodiversity
				Data.LandUse = d[0];
				Data.Pollution = d[1];
				Data.Biodiversity = d[2];
				Data.ImportPollution = d[4];

				// Update the UI
				SetEnvironmentInfo();

				// Update the bar value
				_UpdateBarValue(InfoType.ENVIRONMENT, d[3]);
				_UpdateBarValue(InfoType.POLLUTION, Data.Pollution + Data.ImportPollution);

				// Update the bar slider
				_UpdateBarSlider(
					InfoType.ENVIRONMENT, 
					0.75f
				);
				_UpdateBarSlider(
					InfoType.POLLUTION,
					0.0f
				);
				break;
			case InfoType.MONEY:
				// Sanity check, make sure that there are enough fields
				Debug.Assert(d.Length >= InfoData.N_MONEY_FIELDS);

				// Set the fields in order, for money it's budget, production, building
				Data.Budget = d[0];
				Data.Production = d[1];
				Data.Building = d[2];
				Data.Money = d[3];
				Data.Imports = d[4];

				// Update the UI 
				SetMoneyInfo();
				break;
			default:
				break;
		}
	}

	// Retrieves the import percentage selected by the user
	public float _GetImportSliderPercentage() => (float)Imports._GetImportValue() / 100.0f;

	// ==================== Internal Helpers ====================

	// Sets the required imports based on the demand
	private void SetTargetImport() {
		// Fetch the demand and supply
		float demand  = C._GetDemand().Item1;
		float supply = C._GetGL()._GetResources().Item1.SupplyWinter;

		// Compute the different, clamped to 0 as no imports are required
		// when the supply meets the demand
		float diff = Math.Max(0.0f, demand - supply); 

		Debug.Print("SUPPLY = " + supply + ", DEMAND = " + demand);

		// Compute the percentage of the total demand tha the diff represents
		float diff_perc = diff / demand;

		Debug.Print("DIFF_PERC = " + diff_perc);

		// Set the import target to that percentage
		Imports._UpdateTargetImport(diff_perc);
	}

	// Sets the energy in
	private void SetEnergyInfo(ref InfoBar eng, InfoType t) {
		// Sanity check
		Debug.Assert(t == InfoType.W_ENGERGY || t == InfoType.S_ENGERGY);

		// Extract data based on energy type
		int demand = t == InfoType.W_ENGERGY ? Data.W_EnergyDemand : Data.S_EnergyDemand;
		int supply = t == InfoType.W_ENGERGY ? Data.W_EnergySupply : Data.S_EnergySupply;

		// Get the labels from the XML file
		string demand_label = TC._GetText(LABEL_FILENAME, INFOBAR_GROUP, "label_demand");
		string supply_label = TC._GetText(LABEL_FILENAME, INFOBAR_GROUP, "label_supply");

		// Set the info
		eng._UpdateInfo(
			" ", // N/Max TODO: Figure out what to use here
			demand_label, demand.ToString(), // T0, N0
			supply_label, supply.ToString() // T1, N1
		);
	}

	// Sets the information fields for the support bar
	private void SetSupportInfo() {
		// Get the labels from the XML file
		string afford_label = TC._GetText(LABEL_FILENAME, INFOBAR_GROUP, "label_afford");
		string aesth_label = TC._GetText(LABEL_FILENAME, INFOBAR_GROUP, "label_aesth");

		SupportBar._UpdateInfo(
			" ", // N/Max TODO: Figure out what to use here
			afford_label, Data.EnergyAffordability.ToString(), // T0, N0
			aesth_label, Data.EnvAesthetic.ToString() // T1, N1
		);
	}

	// Sets the information fields for the environment bar
	private void SetEnvironmentInfo() {
		// Get the labels from the XML file
		string land_label = TC._GetText(LABEL_FILENAME, INFOBAR_GROUP, "label_land");
		string buidiv_label = TC._GetText(LABEL_FILENAME, INFOBAR_GROUP, "label_biodiversity");

		EnvironmentBar._UpdateInfo(
			" ", // N/Max TODO: Figure out what to use here
			land_label, Data.LandUse.ToString() + "%", // T0, N0
			buidiv_label, Data.Biodiversity.ToString() + "%" // T2, N2
		);
	}

	// Sets the information fields for the pollution bar
	private void SetPollutionInfo() {
		string poll_label = TC._GetText(LABEL_FILENAME, INFOBAR_GROUP, "label_pollution");
		string import_label = TC._GetText(LABEL_FILENAME, UI_GROUP, "label_import");

		PollutionBar._UpdateInfo(
			// N/Max TODO: Figure out what to use here
			" ",
			poll_label, Data.Pollution.ToString(), // T0, N0
			import_label, Data.ImportPollution.ToString() // T2, N2
		);
	}

	// Sets the information related to the money metric
	private void SetMoneyInfo() {
		// Query the label xml to get the names
		string money_label = TC._GetText(LABEL_FILENAME, INFOBAR_GROUP, "label_money");
		string budget_label = TC._GetText(LABEL_FILENAME, INFOBAR_GROUP, "label_budget");
		string prod_label = TC._GetText(LABEL_FILENAME, INFOBAR_GROUP, "label_production");
		string build_label = TC._GetText(LABEL_FILENAME, INFOBAR_GROUP, "label_building");
		string import_name = TC._GetText(LABEL_FILENAME, UI_GROUP, "label_import");


		// Set Names
		MoneyNameL.Text = money_label;
		BudgetNameL.Text = budget_label;
		ProdNameL.Text = prod_label;
		BuildNameL.Text = build_label;
		ImportCostNameL.Text = import_name;
		
		// Set Values
		BudgetL.Text = Data.Budget.ToString();
		BuildL.Text = Data.Building.ToString();
		ProdL.Text = Data.Production.ToString();
		MoneyL.Text = Data.Money.ToString();
		ImportCostL.Text = Data.Imports.ToString();
	}
	
	// Sets the correct years on the Next Turn Animation
	public void SetNextYears() {
		// Retrieve the current year
		double Year = Timeline.Value;

		// Set up the animation for the year progression
		Animation Anim = TimelineAP.GetAnimation("NextTurnAnim");

		// Retrieve the animation track related to our current turn progression
		int Track = Anim.FindTrack("Year:text", Animation.TrackType.Value);

		// Retrieve the number of animation keypoints in the current track
		int NKeys = Anim.TrackGetKeyCount(Track);

		// Update the year incrementally during the animation
		for (int i = 0; i < NKeys; i++) {
			Anim.TrackSetKeyValue(Track, i, (Year + i).ToString());
		}
	}

	// ==================== Interaction Callbacks ====================

	// Displays additional details about the money usage
	public void _OnMoneyButtonPressed() {
		// Simply toggle the money info
		if(MoneyInfo.Visible) {
			MoneyInfo.Hide();
		} else {
			// Set the info first
			SetMoneyInfo();

			// Finally display it
			MoneyInfo.Show();
		}
	}

	// Updates the timelines and propagates the request up to the game loop
	public async void _OnNextTurnPressed() {
		
		// Sets the correct years and plays the next turn animation
		if(C._GetRemainingTurns() > 0) {
			SetNextYears();
			TimelineAP.Play("NextTurnAnim");
		}
		
		// Waits for the next turn animation before sending the next turn signal
		await ToSignal(TimelineAP, "animation_finished");
		// Trigger the next turn
		EmitSignal(SignalName.NextTurn);

		// Update the required import target (only in winter due to conservative estimates)
		SetTargetImport();
		
		// Update the Timeline
		Timeline.Value = Math.Min(Timeline.Value + TIMELINE_STEP_SIZE, TIMELINE_MAX_VALUE); 
	}

	// Displays the information box related to the winter energy
	public void _OnWinterEnergyMouseEntered() {
		 SetEnergyInfo(ref WinterEnergy, InfoType.W_ENGERGY);

		 // Display the energy info
		 WinterEnergy._DisplayInfo();
	}
	// Hides the information box related to the winter energy
	public void _OnWinterEnergyMouseExited() {
		WinterEnergy._HideInfo();
	}

	// Displays the information box related to the summer energy
	public void _OnSummerEnergyMouseEntered() {
		SetEnergyInfo(ref SummerEnergy, InfoType.S_ENGERGY);

		// Display the energy info
		SummerEnergy._DisplayInfo();
	}
	// Hides the information box related to the summer energy
	public void _OnSummerEnergyMouseExited() {
		SummerEnergy._HideInfo();
	}

	// Displays the information box related to the environment bar
	public void _OnEnvironmentMouseEntered() {
		// Set the information first
		SetEnvironmentInfo();

		// Then show the info
		EnvironmentBar._DisplayInfo();
	}
	// Hides the information box related to the environment bar
	public void _OnEnvironmentMouseExited() {
		EnvironmentBar._HideInfo();
	}

	// Displays the information box related to the Support bar
	public void _OnSupportMouseEntered() {
		// Set the information first
		SetSupportInfo();

		// Show the new info
		SupportBar._DisplayInfo();
	}
	// Hides the information box related to the Support bar
	public void _OnSupportMouseExited() {
		SupportBar._HideInfo();
	}

	// Displays the information box related to the pollution bar
	public void _OnPollutionMouseEntered() {
		// Make sure that the pollution info is up to date
		SetPollutionInfo();

		// Show the new info
		PollutionBar._DisplayInfo();
	}

	// Hides the information box related to the Pollution bar
	public void _OnPollutionMouseExited() {
		PollutionBar._HideInfo();
	}

	// Shows the policy window
	public void _OnPolicyButtonPressed() {
		// Toggle the window visibility  
		if(PW.Visible) {
			PW._PlayAnim("popup", false);
			PW.Hide();
		} else {
			PW.Show();
			PW._PlayAnim("popup");
		}
	}

	// Toggles the settings box
	public void _OnSettingsButtonPressed() {
		// Check the current visibility of the box and act accordingly
		if(SettingsBox.Visible) {
			SettingsBox.Hide();
		} else {
			SettingsBox.Show();
		}
	}

	// Propagates the language update to the game loop
	public void _OnLanguageButtonPressed() {
		// Move to the next language
		C._NextLanguage();

		// Update the ui
		_UpdateUI();
	}
	
	// Closes the language settings by clicking outside
	public void _OnSettingsClosePressed() {
		SettingsBox.Hide();
	}

	// Propagates an import update to the rest of the system
	public void _OnImportUpdate() {
		// Propagate the request of a resource update to the game loop
		GL._UpdateResourcesUI();
	}

	// Propagates a predictive update to the UI
	public void _OnUpdatePrediction() {
		// Get predictive values
		var (MW, MS) = C._GetModels();

		// Update prediction bars
		WinterEnergyPredict._UpdateProgress((int)MW._GetTotalSupply());
		SummerEnergyPredict._UpdateProgress((int)MS._GetTotalSupply());

	}

	// Updates the state of the next turn button
	public void _OnNextTurnStateUpdate(bool state) {
		NextTurnButton.Disabled = state;
	}
}
