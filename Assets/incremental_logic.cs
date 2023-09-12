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
    public TextMeshProUGUI time_text;
    public TextMeshProUGUI steps_text;

    public InputAction stepAction;
    //public int steps;
    private int prev_steps_value=0;
    private int base_steps_value;
    private bool _initial_steps_value_was_set=false;
    private int steps_since_game_started = 0;

    public TextMeshProUGUI steps_since_start_text;
    private int gold=0;

    public TextMeshProUGUI permission_text;
    public TextMeshProUGUI gold_text;
    public TextMeshProUGUI gold_per_step_text;
    public TextMeshProUGUI base_step_value_text;

    int _game_was_ran_before = 0; // indicates that this is the first time the player opens the game    

    
    ///  buyable trees, orchards, forests logic    
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
    /// end buyable trees, orchards, forests logic

    private int gold_per_step = 1;
    private int increment_per_click = 1;


    void Awake()
    {
        AndroidRuntimePermissions.RequestPermission("android.permission.ACTIVITY_RECOGNITION");

        //_game_was_ran_before = PlayerPrefs.GetInt("_game_was_ran_before");

        //if (_game_was_ran_before == 0)
        //{
        //    // initialize game
        //    //InputSystem.EnableDevice(AndroidStepCounter.current);
        //    //AndroidStepCounter.current.MakeCurrent();
        //    //base_steps_value = AndroidStepCounter.current.stepCounter.ReadValue();
        //    //gold = -base_steps_value;
        //}
        //else{
        //    LoadGameData();
        //}
        //_game_was_ran_before = 1;
        //PlayerPrefs.SetInt("_game_was_ran_before", _game_was_ran_before);        

    }


    // Start is called before the first frame update
    void Start()
    {
        LoadGameData();
        InputSystem.EnableDevice(AndroidStepCounter.current);
        AndroidStepCounter.current.MakeCurrent();
        //prev_steps_value = AndroidStepCounter.current.stepCounter.ReadValue();        

        if (AndroidRuntimePermissions.CheckPermission("android.permission.ACTIVITY_RECOGNITION"))
        {            
            permission_text.text = "has permission";
        }
        else
        {            
            permission_text.text = "no permission";
        }

    }

    // Update is called once per frame
    void Update()
    {
        TimeSpan time_passed = DateTime.Now - last_update_time;

        // set initial steps value late to avoid a bug where AndroidStepCounter.current.stepCounter.ReadValue() returns 0 because it hasn't started working yet
        //if (!_initial_steps_value_was_set && time_passed.TotalSeconds > 20)
        //{
        //    base_steps_value = AndroidStepCounter.current.stepCounter.ReadValue();
        //    base_step_value_text.text = base_steps_value.ToString();
        //    _initial_steps_value_was_set = true;
        //    gold = -base_steps_value;
        //    steps_since_game_started = -base_steps_value;
        //    return;
        //}

            // Compare the time passed with a TimeSpan representing 1 second
        if (time_passed.TotalSeconds > 1)
        {            
            last_update_time = DateTime.Now; // Update lastUpdateTime for the next comparison
            time_text.text = last_update_time.ToString();
            steps_text.text = (AndroidStepCounter.current.stepCounter.ReadValue()-base_steps_value).ToString();
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
        // there's no reason for it to ever return 0, so this line AndroidStepCounter.current.stepCounter.ReadValue()!=0 avoids the bug
        if (_initial_steps_value_was_set == false && AndroidStepCounter.current.stepCounter.ReadValue()!=0)
        {
            base_steps_value = AndroidStepCounter.current.stepCounter.ReadValue();
            base_step_value_text.text = base_steps_value.ToString();
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

    //increment number test
    public void incrementNum()
    {
        UpdateGoldPerStep();
        gold+= increment_per_click;
        UpdateGui();
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
        //apple_trees_cost = PlayerPrefs.GetInt("apple_trees_cost");
        //apple_groves_cost = PlayerPrefs.GetInt("apple_groves_cost");
        //apple_forests_cost = PlayerPrefs.GetInt("apple_forests_cost");

        int intValue = PlayerPrefs.GetInt("won_game"); // PlayerPrefs has no bool
        won_game = intValue == 1;
        int intValue2 = PlayerPrefs.GetInt("_initial_steps_value_was_set"); // PlayerPrefs has no bool
        _initial_steps_value_was_set = intValue2 == 1;
        if (_initial_steps_value_was_set)
        {
            base_steps_value = PlayerPrefs.GetInt("base_steps_value");
            //gold = -base_steps_value;
            //steps_since_game_started = -base_steps_value;
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
        //apple_trees_cost = (int)(apple_trees_cost*1.1);
        UpdateGoldPerStep();
        UpdateGui();
    }

    // buy apple groves button
    public void BuyAppleGrove()
    {
        gold -= GetGroveBuyCost();
        apple_groves++;        
        //apple_groves_cost = (int)(apple_groves_cost * 1.1);
        UpdateGoldPerStep();
        UpdateGui();
    }

    // buy apple forest button
    public void BuyAppleForest()
    {
        
        gold -= GetForestsBuyCost();
        apple_forests++;
        //apple_forests_cost = (int)(apple_forests_cost * 1.1);
        UpdateGoldPerStep();
        UpdateGui();
    }

    // win game button
    public void BuyWinButton()
    {
        won_game = true;
        UpdateGui();
    }

    //// end button logic

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
    // end cost of buying a building



    // update how much gold you get per step
    private void UpdateGoldPerStep()
    {
        gold_per_step = 1;
        gold_per_step += apple_trees * 1 + apple_groves * 5 + apple_forests * 25;

        //increment_per_click = 1;
        //increment_per_click += apple_trees * 1 + apple_groves * 5 + apple_forests * 25;        
    }



}

