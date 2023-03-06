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

    public class EventSystem {
        
        public delegate bool EventCondition(EventContext context);
        public delegate void EventCallback(EventContext context);

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

        public void AddListener(string name, EventName eventName, EventCondition condition, EventCallback callback, bool persistListener = false) {
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
        }

        public void AddListener(string name, EventName eventName, bool conditionReturn, EventCallback callback, bool persistListener = false) {
            if (conditionReturn == false) return;
            AddListener(name, eventName, (context) => { return conditionReturn; }, callback, persistListener);
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

        void TriggerEvent(EventName eventName, EventContext context) {
            List<string> listenerKeys = listenerKeysByEvent[eventName];
            Debug.Log("Triggering event: " + eventName + " with " + listenerKeys.Count + " listeners");
            foreach (string listenerKey in listenerKeys) {
                List<EventListener> listeners = eventListenersByKey[listenerKey];
                foreach (EventListener listener in listeners) {
                    if (listener.eventName == eventName && listener.condition(context)) {
                        listener.callback(context);
                    }
                }
            }
        }

        public void TriggerEvent(EventName eventName, params object[] args) {
            EventContext context = new EventContext(model, args);
            TriggerEvent(eventName, context);
        }

        public EventSystem(GameModel model) {
            this.model = model;
            listenerKeysByEvent.Clear();
            eventListenersByKey.Clear();
            foreach (EventName eventName in System.Enum.GetValues(typeof(EventName))) {
                listenerKeysByEvent.Add(eventName, new List<string>());
            }
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