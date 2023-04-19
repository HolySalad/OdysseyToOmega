using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Ship.Activatables;
using SpaceBoat.PlayerSubclasses.Equipment;

namespace SpaceBoat {
        public enum EventName {
            OnGameStart,
            OnGamePause,
            OnGameUnpause,
            OnGameOver,
            OnGameWin,
            OnGameSave,
            OnPlayerDamage,
            OnPlayerDeath,
            OnPlayerHeal,
            OnPlayerJumps,
            OnPlayerLands,
            OnPlayerHeadbump,
            OnPlayerStateChange,
            OnActivatableActivate,
            OnActivatableDeactivate,
            OnActivatableZoneEnter,
            OnActivatableZoneExit,
            OnSaiActivatableBroken,
            OnSailActivatableRepaired,
            OnCraftBlueprintFound,
            OnCraftBlueprintCrafted,
            OnEquipmentUnlock,
            OnEquipmentEquip,
            OnEquipmentUnequip,
            OnBuildableStart,
            OnBuildableConfirm,
            OnBuildableCancel,
            OnHazardBegin,
            OnHazardEnd,
        }

    // a master event system is created under the GameModel which recieves all possible events. 


    public class EventSystem {
        public static EventSystem Instance;
        private static Dictionary<EventSystem, GameObject> gameObjectByEventSystem = new Dictionary<EventSystem, GameObject>();
        private static Dictionary<EventName, List<EventSystem>> eventSystemsByEvent = new Dictionary<EventName, List<EventSystem>>();
        private static Dictionary<EventSystem, List<EventName>> eventsByEventSystem = new Dictionary<EventSystem, List<EventName>>();
        public delegate bool EventCondition(EventContext context);
        public delegate void EventCallback(EventContext context);
        private int uuid = 0;
        private struct EventListener {
            public string name;
            public EventName eventName;
            public EventCondition condition;
            public EventCallback callback;
            public bool persistListener;
        }

        private Dictionary<string, List<EventListener>> eventListenersByKey = new Dictionary<string, List<EventListener>>();
        private Dictionary<EventName, List<string>> listenerKeysByEvent = new Dictionary<EventName, List<string>>();

        private GameModel model;
        private GameObject gameObject;
        private bool isMaster = false;

        public void AddListener(string name, EventName eventName, EventCondition condition, EventCallback callback, bool persistListener = true) {
            EventListener listener = new EventListener();
            listener.name = name;
            listener.eventName = eventName;
            listener.condition = condition;
            listener.callback = callback;
            listener.persistListener = persistListener;
            if (!eventListenersByKey.ContainsKey(name)) {
                eventListenersByKey.Add(name, new List<EventListener>());
            }
            eventListenersByKey[name].Add(listener);
            if (!listenerKeysByEvent[eventName].Contains(name)) {
                listenerKeysByEvent[eventName].Add(name);
            }
            if (!isMaster && !eventsByEventSystem[this].Contains(eventName)) {
                WhiteListEvent(eventName);
            }
        }

        public void AddListener(string name, EventName eventName, bool conditionReturn, EventCallback callback, bool persistListener = true) {
            if (conditionReturn == false) return;
            AddListener(name, eventName, (context) => { return conditionReturn; }, callback, persistListener);
        }

        public int AddListener(EventName eventName, bool conditionReturn, EventCallback callback, bool persistListener = true) {
            if (conditionReturn == false) return -1;
            uuid++;
            AddListener("Unlabeled:"+eventName.ToString()+":"+uuid.ToString(), eventName, (context) => { return conditionReturn; }, callback, persistListener);
            return uuid;
        }

        public int AddListener(EventName eventName, EventCondition condition, EventCallback callback, bool persistListener = true) {
            uuid++;
            AddListener("Unlabeled:"+eventName.ToString()+":"+uuid.ToString(), eventName, condition, callback, persistListener);
            return uuid;
        }


        public void RemoveListener(string name) {
            List<EventListener> listeners = eventListenersByKey[name];
            List<EventName> eventNames = new List<EventName>();
            foreach (EventListener listener in listeners) {
                eventNames.Add(listener.eventName);
            }
            foreach (EventName eventName in eventNames) {
                listenerKeysByEvent[eventName].Remove(name);
            }
            eventListenersByKey[name].Clear();
        }

        public void RemoveListener(int uuid, EventName eventName) {
            RemoveListener("Unlabeled:"+eventName.ToString()+":"+uuid.ToString());
        }


        void TriggerEvent(EventName eventName, EventContext context, bool triggerIfNotMaster) {
            if (!(isMaster||triggerIfNotMaster)) {
                Debug.LogError("EventSystem.TriggerEvent() should only be called on the master EventSystem");
                return;
            }
            List<string> listenerKeys = listenerKeysByEvent[eventName];
            Debug.Log("Triggering event: " + eventName + " with " + listenerKeys.Count + " listeners in the master event system");
            List<EventListener> listenersToRemove = new List<EventListener>();
            foreach (string listenerKey in listenerKeys) {
                List<EventListener> listeners = eventListenersByKey[listenerKey];
                foreach (EventListener listener in listeners) {
                    if (listener.eventName == eventName && listener.condition(context)) {
                        listener.callback(context);
                        if (listener.persistListener == false) {
                            listenersToRemove.Add(listener);
                        }
                    }
                }
            }
            foreach (EventListener listener in listenersToRemove) {
                RemoveListener(listener.name);
            }
            if (!isMaster) {
                return;
            }
            if (eventSystemsByEvent.ContainsKey(eventName)) {
                List<EventSystem> eventSystemsToRemove = new List<EventSystem>();
                foreach (EventSystem eventSystem in eventSystemsByEvent[eventName]) {
                    //check if the associated gameobject is still active
                    if (gameObjectByEventSystem[eventSystem] == null) {
                        eventSystemsToRemove.Add(eventSystem);
                    } else {
                        if (gameObjectByEventSystem[eventSystem].activeInHierarchy) {
                            eventSystem.TriggerEvent(eventName, context, true);
                        }
                        //ignore inactive ones.
                    }
                }
                foreach (EventSystem eventSystem in eventSystemsToRemove) {
                    DestroyChildEventSystem(eventSystem);
                }
            }
        }

        public void TriggerEvent(EventName eventName, params object[] args) {
            EventContext context = new EventContext(model, args);
            TriggerEvent(eventName, context, false);
        }
        public EventSystem(GameModel model) {
            this.model = model;
            this.gameObject = model.gameObject;
            Instance = this;
            isMaster = true;
            listenerKeysByEvent.Clear();
            eventListenersByKey.Clear();
            foreach (EventName eventName in System.Enum.GetValues(typeof(EventName))) {
                listenerKeysByEvent.Add(eventName, new List<string>());
            }
        }

        public EventSystem(GameObject obj) {
            gameObject = obj;
            gameObjectByEventSystem.Add(this, obj);
            eventsByEventSystem.Add(this, new List<EventName>());
            listenerKeysByEvent.Clear();
            eventListenersByKey.Clear();
            model = Instance.model;
            foreach (EventName eventName in System.Enum.GetValues(typeof(EventName))) {
                listenerKeysByEvent.Add(eventName, new List<string>());
            }
            
        }

        public void WhiteListEvent(EventName eventName) {
            if (isMaster) {
                Debug.LogError("Master event system should not be whitelisting events");
                return;
            }
            if (!eventSystemsByEvent.ContainsKey(eventName)) {
                eventSystemsByEvent.Add(eventName, new List<EventSystem>());
            }
            eventSystemsByEvent[eventName].Add(this);
            eventsByEventSystem[this].Add(eventName);
        }

        public void WhiteListEvent(EventName[] eventNames) {
            if (isMaster) {
                Debug.LogError("Master event system should not be whitelisting events");
                return;
            }
            foreach (EventName eventName in eventNames) {
                if (!eventSystemsByEvent.ContainsKey(eventName)) {
                    eventSystemsByEvent.Add(eventName, new List<EventSystem>());
                }
                eventSystemsByEvent[eventName].Add(this);
                eventsByEventSystem[this].Add(eventName);
            }
        }

        public static void DestroyChildEventSystem(EventSystem system) {
            if (system.isMaster) {
                Debug.LogError("Master event system should not be destroyed");
                return;
            }
            foreach (EventName eventName in eventsByEventSystem[system]) {
                eventSystemsByEvent[eventName].Remove(system);
            }
            eventsByEventSystem.Remove(system);
            gameObjectByEventSystem.Remove(system);
        }
    }
    public class EventContext {
        private GameModel model;
        public GameModel Model {get {return model;}}
        private Player player;
        public Player Player {get {return player;}}
        private Player.PlayerMovementInfo playerMovementInfo;
        public Player.PlayerMovementInfo PlayerMovementInfo {get {return playerMovementInfo;}}
        private PlayerStateName playerState;
        public PlayerStateName PlayerState {get {return playerState;}}
        private IActivatables activatable;
        public IActivatables Activatable {get {return activatable;}}
        private Rewards.ICraftBlueprint craftBlueprint;
        public Rewards.ICraftBlueprint CraftBlueprint {get {return craftBlueprint;}}
        private IPlayerEquipment equipment;
        public IPlayerEquipment Equipment {get {return equipment;}}
        private Ship.Buildables.IBuildable buildable;
        public Ship.Buildables.IBuildable Buildable {get {return buildable;}}

        


        public EventContext(GameModel model, params object[] args) {
            this.model = model;
            //this is kinda ugly but I'm not sure if there is a cleaner way to do this.
            foreach (object arg in args) {
                switch (arg) {
                    case Player player:
                        this.player = player;
                        break;
                    case Player.PlayerMovementInfo playerMovementInfo:
                        this.playerMovementInfo = playerMovementInfo;
                        break;
                    case PlayerStateName playerState:
                        this.playerState = playerState;
                        break;
                    case IActivatables activatable:
                        this.activatable = activatable;
                        break;
                    case Rewards.ICraftBlueprint craftBlueprint:
                        this.craftBlueprint = craftBlueprint;
                        break;
                    case IPlayerEquipment equipment:
                        this.equipment = equipment;
                        break;
                    case Ship.Buildables.IBuildable buildable:
                        this.buildable = buildable;
                        break;
                    default:
                        Debug.LogWarning("EventContext: Unhandled argument type: " + arg.GetType());
                        break;
                }
            }
        }
    }
}