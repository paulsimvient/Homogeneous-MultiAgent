using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InitDropdownVariables : MonoBehaviour
{
    Dropdown dropdown;

    void Awake()
    {
        BEBlock thisBlock = GetComponent<BEBlock>();
        thisBlock.InitializeBlock();

        dropdown = thisBlock.BlockHeader.GetChild(thisBlock.userInputIndexes[0]).GetComponent<Dropdown>();
    
        //populate dropdown
        dropdown.ClearOptions();
        
        foreach (BEVariable variable in thisBlock.BeController.BeVariableList)
        {
            dropdown.options.Add(new Dropdown.OptionData(variable.name));
        }
        dropdown.RefreshShownValue();
    
        BEController.BeVariableListChangeEvent += RepopulateDropdown;
    }

    // v1.3 -Bug fix: [reported on Unity 2019] variable blocks dropdown changing to index 0 on Play pressed
    public void RepopulateDropdown()
    {
        BEBlock thisBlock = GetComponent<BEBlock>();

        int selectedValue = dropdown.value;
        
        dropdown.ClearOptions();
        foreach (BEVariable variable in thisBlock.BeController.BeVariableList)
        {
            dropdown.options.Add(new Dropdown.OptionData(variable.name));
        }
        dropdown.RefreshShownValue();
        
        dropdown.value = selectedValue;
    }

    private void OnDestroy()
    {
        BEController.BeVariableListChangeEvent -= RepopulateDropdown;
    }
}
