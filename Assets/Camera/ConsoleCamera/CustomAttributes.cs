using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CustomAttributes { }

[AttributeUsage(AttributeTargets.Field)]
public class CreateSlider : Attribute
{
    private static AttributeSlider attributePrefab { get { return Resources.Load<AttributeSlider>("AttributeSlider"); } }
    public readonly string name;
    public readonly float min;
    public readonly float max;
    public readonly bool wholeNumbers;
    public readonly bool displayValue;
    public readonly Color textColor;
    public string onChangeMethod;



    public CreateSlider(string name, float min, float max, bool wholeNumbers = false, bool displayValue = true, bool alternateColor = false, string method = "")
    {
        this.name = name;  
        this.min = min;
        this.max = max;
        this.wholeNumbers = wholeNumbers;
        this.displayValue = displayValue;
        this.textColor = alternateColor ? Color.black : Color.white;
        this.onChangeMethod = method;

    }

    public static void CreateAttribute(UnityEngine.Object obj, Transform sliderContainer)
    {
        Type monoType = obj.GetType();

        foreach (var field in monoType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var attributes = field.GetCustomAttributes(typeof(CreateSlider), true);
            if (attributes.Length > 0)
            {
                for (int i = 0; i < attributes.Length; i++)
                {
                    CreateSlider attSlider = attributes[i] as CreateSlider;
                    
                    AttributeSlider instance = null;
                    if (attributePrefab != null)
                        instance = MonoBehaviour.Instantiate(attributePrefab, sliderContainer) as AttributeSlider;
                    else Debug.LogError("Cant find attribute slider prefab, check path");

                    if (instance)
                    {
                        if(attSlider.wholeNumbers)
                            instance.slider.value = (int)field.GetValue(obj);
                        else instance.slider.value = (float)field.GetValue(obj);
                        instance.slider.minValue = attSlider.min;
                        instance.slider.maxValue = attSlider.max;
                        instance.slider.wholeNumbers = attSlider.wholeNumbers;
                        instance.title.text = attSlider.name;
                        instance.title.color = attSlider.textColor;
                        instance.displayedValue.gameObject.SetActive(attSlider.displayValue);

                        instance.slider.onValueChanged.AddListener
                        (
                            delegate
                            {
                                if (instance.slider.wholeNumbers)
                                    field.SetValue(obj, (int)instance.slider.value);
                                else field.SetValue(obj, (float)instance.slider.value);

                                if (!String.IsNullOrEmpty(attSlider.onChangeMethod))
                                {
                                    MethodInfo mi = monoType.GetMethod(attSlider.onChangeMethod);
                                    if(mi != null)
                                    {
                                        mi.Invoke(obj, null);
                                    }
                                }
                            }
                        );
                    }
                }
            }
        }
    }
}
