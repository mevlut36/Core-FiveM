using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Client
{
    public class NUIManager : BaseScript
    {
        private static NUIManager _instance;
        private bool _isNuiOpen = false;
        private string _currentNuiName = "";
        private Dictionary<string, Action<object>> _callbacks = new Dictionary<string, Action<object>>();

        public static NUIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NUIManager();
                }
                return _instance;
            }
        }

        public NUIManager()
        {
            _instance = this;

            API.RegisterNuiCallbackType("closeNUI");
            API.RegisterNuiCallbackType("nuiCallback");

            EventHandlers["__cfx_nui:closeNUI"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnCloseNUI);
            EventHandlers["__cfx_nui:nuiCallback"] += new Action<IDictionary<string, object>, CallbackDelegate>(OnNuiCallback);
        }

        public void OpenNUI(string nuiName, object data = null)
        {
            if (_isNuiOpen)
            {
                Debug.WriteLine($"[NUI] Une interface est déjà ouverte: {_currentNuiName}");
                return;
            }

            _isNuiOpen = true;
            _currentNuiName = nuiName;

            SetNuiFocus(true, true);

            SendNUIMessage(new
            {
                action = "open",
                nui = nuiName,
                data
            });

            Debug.WriteLine($"[NUI] Interface ouverte: {nuiName}");
        }

        public void CloseNUI()
        {
            Debug.WriteLine($"[NUI] CloseNUI - IsOpen: {_isNuiOpen}, Current: {_currentNuiName}");

            API.SetNuiFocus(false, false);
            API.SetNuiFocusKeepInput(false);

            SendNUIMessage(new { action = "close" });

            _isNuiOpen = false;
            _currentNuiName = "";

            BaseScript.TriggerEvent("nui:closed");

            Debug.WriteLine("[NUI] Fermé");
        }

        public void SendNUIMessage(object data)
        {
            string json = JsonConvert.SerializeObject(data);
            API.SendNuiMessage(json);
        }

        public void RegisterCallback(string callbackName, Action<object> callback)
        {
            if (_callbacks.ContainsKey(callbackName))
            {
                _callbacks[callbackName] = callback;
            }
            else
            {
                _callbacks.Add(callbackName, callback);
            }
        }

        public void UnregisterCallback(string callbackName)
        {
            if (_callbacks.ContainsKey(callbackName))
            {
                _callbacks.Remove(callbackName);
            }
        }

        private void SetNuiFocus(bool hasFocus, bool hasCursor)
        {
            API.SetNuiFocus(hasFocus, hasCursor);
            API.SetNuiFocusKeepInput(false);
        }

        private void OnCloseNUI(IDictionary<string, object> data, CallbackDelegate callback)
        {
            CloseNUI();
            callback(new { ok = true });
        }

        private void OnNuiCallback(IDictionary<string, object> data, CallbackDelegate callback)
        {
            Debug.WriteLine("=== [NUIManager] OnNuiCallback APPELÉ ===");
            Debug.WriteLine($"[NUIManager] Données reçues : {JsonConvert.SerializeObject(data)}");

            try
            {
                string callbackName = data.ContainsKey("callback") ? data["callback"].ToString() : "";

                Debug.WriteLine($"[NUIManager] Callback recherché : '{callbackName}'");

                if (callbackName.StartsWith("concess:"))
                {
                    Debug.WriteLine($"[NUIManager] Callback ConcessAuto détecté");

                    switch (callbackName)
                    {
                        case "concess:previewVehicle":
                            if (data.ContainsKey("model"))
                            {
                                TriggerEvent("concess:previewVehicle", data["model"].ToString());
                            }
                            break;

                        case "concess:setPrimaryColor":
                            if (data.ContainsKey("colorId"))
                            {
                                TriggerEvent("concess:setPrimaryColor", Convert.ToInt32(data["colorId"]));
                            }
                            break;

                        case "concess:setSecondaryColor":
                            if (data.ContainsKey("colorId"))
                            {
                                TriggerEvent("concess:setSecondaryColor", Convert.ToInt32(data["colorId"]));
                            }
                            break;

                        case "concess:setWheels":
                            if (data.ContainsKey("wheelType") && data.ContainsKey("wheelIndex"))
                            {
                                TriggerEvent("concess:setWheels",
                                    Convert.ToInt32(data["wheelType"]),
                                    Convert.ToInt32(data["wheelIndex"]));
                            }
                            break;

                        case "concess:buyVehicle":
                            TriggerEvent("concess:buyVehicle");
                            break;

                        case "concess:closePreview":
                            TriggerEvent("concess:closePreview");
                            break;
                    }

                    callback(new { ok = true });
                    return;
                }

                if (_callbacks.ContainsKey(callbackName))
                {
                    Debug.WriteLine($"[NUIManager] Callback trouvé");
                    _callbacks[callbackName]?.Invoke(data);
                    callback(new { ok = true });
                }
                else
                {
                    Debug.WriteLine($"[NUIManager] Callback '{callbackName}' non trouvé");
                    callback(new { ok = false, error = "Callback not found" });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NUIManager] ERREUR: {ex.Message}");
                callback(new { ok = false, error = ex.Message });
            }
        }

        public bool IsOpen => _isNuiOpen;
        public string CurrentNUI => _currentNuiName;
    }
}