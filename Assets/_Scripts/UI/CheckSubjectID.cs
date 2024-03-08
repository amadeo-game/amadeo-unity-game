using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckSubjectID : MonoBehaviour
{
    [SerializeField] InputField subjectID;
    [SerializeField] TextMeshPro errorText;

    [SerializeField] private GameObject form;
// check if the subject ID is valid, should be 4 digits
    public void CheckID()
    {
        if (subjectID.text.Length != 4)
        {
            errorText.text = "Subject ID should be 4 digits";
        }
        else
        {
            errorText.text = "";
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
