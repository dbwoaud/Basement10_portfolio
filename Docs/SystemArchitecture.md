graph TD
    subgraph Input_Layer
        PI[PlayerInput] --> PM[PlayerMovement]
    end

    subgraph Event_System
        ET[EndingTrigger] -- "Action: OnEndingTriggered" --> GM
        EC[ElevatorController] -- "Action: OnElevatorAnswerSelected" --> GM
    end

    subgraph Global_Managers [Singleton]
        GM[GameManager] <--> SM[SoundManager]
        GM <--> FM[FadeManager]
        GM --> SAM[SpawnAbnormalManager]
    end

    subgraph Content_Logic
        SAM --> AD[AbnormalData: ScriptableObject]
        AD --> MAP[Current Map Instance]
    end

    subgraph UI_Layer
        GM --> SUI[StoryModeUIManager]
        BEM[BadEndingManager] --> BU[BadEndingUIManager]
        TEM[TrueEndingManager] --> TU[TrueEndingUIManager]
    end
