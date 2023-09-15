using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using AndroidRuntimePermissionsNamespace;
using UnityEngine.Android;


using UnityEngine.InputSystem.Android;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class game_logic : MonoBehaviour
{   
    DateTime last_update_time = DateTime.Now;

    public InputAction stepAction;
    //public int steps;
    private int prev_steps_value=0; // AndroidStepCounter.current.stepCounter.ReadValue() - prev_steps_value = number of steps the player walked since last time we checked

    //AndroidStepCounter.current.stepCounter.ReadValue() returns the total number of steps since phone was turned on
    // we want to ignore the first steps made before the game was started
    private int base_steps_value; // the number of steps made before the game was started
    private bool _initial_steps_value_was_set=false; //flag that base_steps_value was set
    private int steps_since_game_started = 0;

    public TextMeshProUGUI steps_since_start_text; // total steps since the game was first started, due to api limits this will have issus if the phone is restarted (true on android and ios)
    private int gold=0;

    
    public TextMeshProUGUI gold_text;
    public TextMeshProUGUI gold_per_step_text;


    ///  object the player buys - trees, orchards, forests
    public UnityEngine.UI.Button buy_tree_button;
    public TextMeshProUGUI buy_tree_button_text;
    private int apple_trees = 0;
    private int apple_trees_cost = 50;
    public TextMeshProUGUI apple_trees_count_text;    

    public UnityEngine.UI.Button buy_grove_button;
    public TextMeshProUGUI buy_grove_button_text;
    private int apple_groves = 0;
    private int apple_groves_cost = 250;
    public TextMeshProUGUI apple_groves_count_text;

    public UnityEngine.UI.Button buy_forest_button;
    public TextMeshProUGUI buy_forest_button_text;
    private int apple_forests = 0;
    private int apple_forests_cost = 1250;
    public TextMeshProUGUI apple_forests_count_text;

    public UnityEngine.UI.Button win_game_button;
    public TextMeshProUGUI win_game_button_text;
    private int win_game_cost = 500000;
    private bool won_game= false;
    

    private int gold_per_step = 1;

    void Awake()
    {
        AndroidRuntimePermissions.RequestPermission("android.permission.ACTIVITY_RECOGNITION");

    }


    // Start is called before the first frame update
    void Start()
    {
        LoadGameData();
        InputSystem.EnableDevice(AndroidStepCounter.current);
        AndroidStepCounter.current.MakeCurrent();        

        if (AndroidRuntimePermissions.CheckPermission("android.permission.ACTIVITY_RECOGNITION"))
        {            
            //permission_text.text = "has permission";
        }
        else
        {            
            //permission_text.text = "no permission";
        }

    }

    // Update is called once per frame
    void Update()
    {
        TimeSpan time_passed = DateTime.Now - last_update_time;

        // Compare the time passed with a TimeSpan representing 1 second
        if (time_passed.TotalSeconds > 1)
        {            
            last_update_time = DateTime.Now; // Update lastUpdateTime for the next comparison
            UpdateStepsNumber();
            UpdateGui();
        }

        //disable / enable buttons
        if (gold > GetTreeBuyCost()) buy_tree_button.enabled = true;
        else buy_tree_button.enabled = false;
        if (gold > GetGroveBuyCost()) buy_grove_button.enabled = true;
        else buy_grove_button.enabled = false;
        if (gold > GetForestsBuyCost()) buy_forest_button.enabled = true;
        else buy_forest_button.enabled = false;
        if (gold > win_game_cost) win_game_button.enabled = true;
        else win_game_button.enabled = false;
    }

    // update the number of steps
    void UpdateStepsNumber()
    {
        // for some reason AndroidStepCounter.current.stepCounter.ReadValue() returns 0 for the first few seconds, which causes a bug when setting the initial step value
        // there's no reason for it to ever return 0, so this line AndroidStepCounter.current.stepCounter.ReadValue()!=0 dodges the bug
        if (_initial_steps_value_was_set == false && AndroidStepCounter.current.stepCounter.ReadValue()!=0)
        {
            base_steps_value = AndroidStepCounter.current.stepCounter.ReadValue();            
            _initial_steps_value_was_set = true;
            gold = -base_steps_value;
            steps_since_game_started = -base_steps_value;
            PlayerPrefs.SetInt("base_steps_value", base_steps_value);            
        }
        else
        {
            steps_since_game_started += AndroidStepCounter.current.stepCounter.ReadValue() - prev_steps_value;
            gold += (AndroidStepCounter.current.stepCounter.ReadValue() - prev_steps_value) * gold_per_step;
            prev_steps_value = AndroidStepCounter.current.stepCounter.ReadValue();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // The application is going into the background or being paused
            SaveGameData();
        }
        else
        {
            // The application is being resumed
            LoadGameData();
            UpdateGui();
        }
    }

    private void SaveGameData()
    {
        PlayerPrefs.SetInt("gold", gold);
        PlayerPrefs.SetInt("apple_trees", apple_trees);
        PlayerPrefs.SetInt("apple_groves", apple_groves);
        PlayerPrefs.SetInt("apple_forests", apple_forests);

        int int_won_game = won_game ? 1 : 0; // PlayerPrefs has no bool
        PlayerPrefs.SetInt("won_game", int_won_game);
        int int__initial_steps_value_was_set = _initial_steps_value_was_set ? 1 : 0; // PlayerPrefs has no bool
        PlayerPrefs.SetInt("_initial_steps_value_was_set", int__initial_steps_value_was_set);

        if(_initial_steps_value_was_set) PlayerPrefs.SetInt("base_steps_value", base_steps_value);

        PlayerPrefs.SetInt("prev_steps_value", prev_steps_value);        

        PlayerPrefs.Save();
    }

    private void LoadGameData()
    {
        gold = PlayerPrefs.GetInt("gold");
        apple_trees = PlayerPrefs.GetInt("apple_trees");
        apple_groves = PlayerPrefs.GetInt("apple_groves");
        apple_forests = PlayerPrefs.GetInt("apple_forests");

        int intValue = PlayerPrefs.GetInt("won_game"); // PlayerPrefs has no bool
        won_game = intValue == 1;
        int intValue2 = PlayerPrefs.GetInt("_initial_steps_value_was_set"); // PlayerPrefs has no bool
        _initial_steps_value_was_set = intValue2 == 1;
        if (_initial_steps_value_was_set)
        {
            base_steps_value = PlayerPrefs.GetInt("base_steps_value");
        }

        prev_steps_value = PlayerPrefs.GetInt("prev_steps_value");
        UpdateGoldPerStep();
    }

    private void UpdateGui()
    {        
        
        /// update the text of the buttons
        apple_trees_count_text.text = apple_trees.ToString();
        apple_groves_count_text.text = apple_groves.ToString();
        apple_forests_count_text.text = apple_forests.ToString();

        buy_tree_button_text.text = "Buy apple tree (" + GetTreeBuyCost().ToString() + ")";
        buy_grove_button_text.text = "Buy grove (" + GetGroveBuyCost().ToString() + ")";
        buy_forest_button_text.text = "Buy forest (" + GetForestsBuyCost().ToString() + ")";

        steps_since_start_text.text = steps_since_game_started.ToString();        
        gold_per_step_text.text = gold_per_step.ToString();
        gold_text.text = gold.ToString();

        if (won_game) win_game_button_text.text = "You won the game!";
        else win_game_button_text.text = "Win game (" + win_game_cost.ToString() + ")";
    }



    //// building button logic
    // buy apple trees button
    public void BuyAppleTree()
    {
        gold -= GetTreeBuyCost();
        apple_trees++;                
        UpdateGoldPerStep();
        UpdateGui();
    }

    // buy apple groves button
    public void BuyAppleGrove()
    {
        gold -= GetGroveBuyCost();
        apple_groves++;                
        UpdateGoldPerStep();
        UpdateGui();
    }

    // buy apple forest button
    public void BuyAppleForest()
    {
        
        gold -= GetForestsBuyCost();
        apple_forests++;        
        UpdateGoldPerStep();
        UpdateGui();
    }

    // win game button
    public void BuyWinButton()
    {
        won_game = true;
        UpdateGui();
    }


    // get cost of buying a building
    private int GetTreeBuyCost()
    {        
        return (int) (apple_trees_cost * (float) Math.Pow(1.1, apple_trees));        
    }
    private int GetGroveBuyCost()
    {
        return (int) (apple_groves_cost * (float)Math.Pow(1.1, apple_groves));
    }
    private int GetForestsBuyCost()
    {
        return (int) (apple_forests_cost * (float)Math.Pow(1.1, apple_forests));
    }


    // update how much gold the player gets per step
    private void UpdateGoldPerStep()
    {
        gold_per_step = 1;
        gold_per_step += apple_trees * 1 + apple_groves * 5 + apple_forests * 25;
    }



}

