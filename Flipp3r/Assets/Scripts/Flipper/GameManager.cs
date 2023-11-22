using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections.Concurrent;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public enum GameState {
    Standby,
    Booting,
    Start,
    Game,
    BeforeGameOver,
    GameOver,
    Victory,
    End
}

public class GameManager : MonoBehaviour {

    public GameState State { get; set; }

    public static GameManager Instance;

    // Score
    public int[] scores;
    private readonly object score_lock = new object();

    // Game elements
    private List<Ball> balls;
    private List<Bumper> bumpers;
    private List<Light> lights;
    private List<Rigidbody> rigidbodies;
    private List<MeshRenderer> meshRenderers;

    public float spawnCD = 1f;

    public GameObject Zone1;
    public GameObject Zone2;
    public GameObject Zone3;

    public GameObject Ball1;
    public GameObject Ball2;
    public GameObject Ball3;
    public Transform[] spawns = new Transform[6];
    private ConcurrentDictionary<string, int> numBallsPlayer = new ConcurrentDictionary<string, int>();
    public int maxBallsByPlayer = 3;
    public int totalMaxBalls = 15;
    public int totalBalls = 0;
    public int deadBalls = 0;

    private readonly object lock_1 = new object();
    private readonly object lock_2 = new object();
    
    private int bossLife;
    private bool canWeaknessSet;

    // Robot animation
    public GameObject bossFace1;
    public GameObject bossFace2;
    public GameObject bossFace3;
    public GameObject bossFace4;
    public GameObject bossFace5;

    // Video players for the different boss phases
    public VideoPlayer standbyPlayer;
    public VideoPlayer bootingPlayer;
    public VideoPlayer[] idlePlayers;
    public VideoPlayer[] tauntPlayers;
    public VideoPlayer[] hitPlayers;
    public VideoPlayer[] destructionPlayers;
    // HP thresholds to change phase
    public int[] thresholds;

    // Index of the videos
    private int idleIndex = 0;
    private int hitIndex = 0;
    private int localHitIndex = 0;
    private int tauntIndex = 0;
    private int destructionIndex = 0;
    private int thresholdIndex = 0;
    // Is an animation playing
    private bool playing = false;
    // Has the boss already taunted in this phase
    private bool taunted = false;
    // Array to keep in memory when all the players are ready, not used anymore
    private bool[] playersReady = {false, false, false};

    // Post processing volume
    public Volume volume;
    VolumeProfile profile;

    // Cameras used to shake the terrain
    public Camera[] terrainToShake;

    // The text used for scores in the UI
    public TextMeshProUGUI[] scoresText;

    // Boss Spline
    public SplineWalkerBoss smokeSpline;
    public SplineWalkerBoss lightSpline;

    // Boss Ball
    public Transform boss;
    public GameObject ballBoss;
    // GameObjects to turn when the players have lost
    public GameObject[] gameOverTurn;
    // Base material to help drawing
    public Material baseMat;
    // Bumpers that can be a weakness in each zone
    public Bumper[] zone1Bumpers;
    public Bumper[] zone2Bumpers;
    public Bumper[] zone3Bumpers;
    // Coroutine of the sake, needs to be a variable so it can be stopped if players lose when coroutine is going on
    private Coroutine shakeCoroutine;
    // Variable to know if terrain is illuminated or not, used for drawing
    private bool illuminated = false;
    public Light illuminatedLight;
    private int ballIndex = 0;
    public GameObject ballsUsed;
    // Center of the terrain, used for the shake
    public GameObject center;

    private bool shaked = false;

    private int tauntEffectCount = 0;

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
            return;
        }
    }

    void Start() {
        // When game is reset, center objects becomes null so we have to find it
        terrainToShake[0] = GameObject.Find("MainCamera").GetComponent<Camera>();
        if (center == null){
            center = GameObject.Find("Center");
        }

        // First state
        UpdateGameState(GameState.Standby);
        terrainToShake[0].transform.LookAt(center.transform);

        // The boss' life
        this.bossLife = 375;

        // Game values
        this.canWeaknessSet = true;
        this.numBallsPlayer["BallP1"] = 0;
        this.numBallsPlayer["BallP2"] = 0;
        this.numBallsPlayer["BallP3"] = 0;
        this.scores = new int[3]; // Not set because default value of integer is 0

        // Game elements
        GetBalls();
        GetBumpers();
        GetLights();
        GetRigidbodies();
        GetMeshRenderers();
        
        // Profile base values
        profile = volume.sharedProfile;
        if (profile.TryGet<ColorAdjustments>(out var ca))
        {
            ca.postExposure.value = 0;
        }
        if (profile.TryGet<HDRISky>(out var sky))
        {
            sky.multiplier.value = 0;
        }
    }

    void Update() {
        // Changing a bumper to attack the boss
        if(this.canWeaknessSet) {
            int chosenIndex = Random.Range(0, this.zone1Bumpers.Length);
            this.zone1Bumpers[chosenIndex].SetWeakness(true);
            chosenIndex = Random.Range(0, this.zone2Bumpers.Length);
            this.zone2Bumpers[chosenIndex].SetWeakness(true);
            chosenIndex = Random.Range(0, this.zone3Bumpers.Length);
            this.zone3Bumpers[chosenIndex].SetWeakness(true);
            this.canWeaknessSet = false;

        }
        // Admin shortcuts
        if(Input.GetKey("left ctrl")) {
            if(Input.GetKeyDown("y")) {
                ResetBalls();
            } else if(Input.GetKeyDown("x")) {
                ResetScore();
            } else if(Input.GetKeyDown("b")) {
                PauseGame();
            } else if(Input.GetKeyDown("n")) {
                ResumeGame();
            } else if(Input.GetKeyDown("v")) {
                ResetGame();
            } else if(Input.GetKeyDown("m")) {
                ChangeToBaseMat();
            }
        }
        if(Input.GetKeyDown(KeyCode.F1)){
            illuminatedLight.enabled = false;
            ResumeGame();
            ResetGame();
        }
        else if(Input.GetKeyDown(KeyCode.F2)){
            if (profile.TryGet<HDRISky>(out var sky))
            {
                if(!illuminated){
                    sky.multiplier.value = 1000;
                    illuminated = true;
                }else{
                    sky.multiplier.value = 0;
                    illuminated = false;
                }
            }
        }
        else if(Input.GetKeyDown(KeyCode.F3)){
            illuminatedLight.shadows = LightShadows.None;
            if(!illuminatedLight.enabled){
                illuminatedLight.enabled = true;
                ChangeToBaseMat();
                this.bossFace1.SetActive(true);
                this.bossFace2.SetActive(false);
                this.bossFace3.SetActive(false);
                this.bossFace4.SetActive(false);
                this.bossFace5.SetActive(false);
            }
            else{
                illuminatedLight.intensity = 20000;
            }
        }
        else if(Input.GetKeyDown(KeyCode.F4) && illuminatedLight.enabled){
            illuminatedLight.shadows = LightShadows.Hard;
            illuminatedLight.gameObject.transform.eulerAngles = new Vector3(30, 0, 0);
            illuminatedLight.intensity = 15000;
        }
        else if(Input.GetKeyDown(KeyCode.F5) && illuminatedLight.enabled){
            illuminatedLight.shadows = LightShadows.Hard;
            illuminatedLight.gameObject.transform.eulerAngles = new Vector3(60, 0, 0);
            illuminatedLight.intensity = 10000;
        }
        else if(Input.GetKeyDown(KeyCode.F6) && illuminatedLight.enabled){
            illuminatedLight.shadows = LightShadows.Hard;
            illuminatedLight.gameObject.transform.eulerAngles = new Vector3(120, 0, 0);
            illuminatedLight.intensity = 15000;
        }
        else if(Input.GetKeyDown(KeyCode.F7) && illuminatedLight.enabled){
            illuminatedLight.shadows = LightShadows.Hard;
            illuminatedLight.gameObject.transform.eulerAngles = new Vector3(150, 0, 0);
            illuminatedLight.intensity = 10000;
        }
        else if (Input.GetKeyDown(KeyCode.F8) && illuminatedLight.enabled)
        {
            this.bossFace1.SetActive(false);
            this.bossFace2.SetActive(true);
            this.bossFace3.SetActive(false);
            this.bossFace4.SetActive(false);
            this.bossFace5.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.F9) && illuminatedLight.enabled)
        {
            this.bossFace1.SetActive(false);
            this.bossFace2.SetActive(false);
            this.bossFace3.SetActive(true);
            this.bossFace4.SetActive(false);
            this.bossFace5.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.F10) && illuminatedLight.enabled)
        {
            this.bossFace1.SetActive(false);
            this.bossFace2.SetActive(false);
            this.bossFace3.SetActive(false);
            this.bossFace4.SetActive(true);
            this.bossFace5.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.F11) && illuminatedLight.enabled)
        {
            this.bossFace1.SetActive(false);
            this.bossFace2.SetActive(false);
            this.bossFace3.SetActive(false);
            this.bossFace4.SetActive(false);
            this.bossFace5.SetActive(true);
        }

        if (State == GameState.Booting) {
            if(Input.GetKey("left ctrl") && Input.GetKeyDown("s")) {
                StartCoroutine(BootingOverCoroutine());
            }
        }else if(State == GameState.Game) {
            if(Input.GetKey("left ctrl") && Input.GetKeyDown("j")) {
                idleIndex = 4;
                destructionIndex = 4;
                hitIndex+=8;
                localHitIndex=0;
                tauntIndex = 4;
                thresholdIndex = 4;
                playing = false;
                taunted = false;
                bossLife = 0;
                
                idlePlayers[idleIndex].gameObject.SetActive(true);
                idlePlayers[idleIndex].Play();
                Transform t0 = idlePlayers[idleIndex].gameObject.transform;
                idleIndex = 0;
                Transform t1 = idlePlayers[idleIndex].gameObject.transform;
                t0.position = t0.position + new Vector3(0,1,0);
                t1.position = t1.position - new Vector3(0,1,0);
                idlePlayers[idleIndex].Stop();
                idlePlayers[idleIndex].gameObject.SetActive(false);
                idleIndex = 4;
            }
        }

        // If a players inputs a key when robot is in standby, game launches
        if ((Input.GetKeyDown("w") || Input.GetKeyDown("a") || Input.GetKeyDown("s") || Input.GetKeyDown("d")
            || Input.GetKeyDown("f") || Input.GetKeyDown("g")) && State == GameState.Standby) {
            UpdateGameState(GameState.Booting);
        }

        // If a player inputs a key when the game is over, game restarts
        if((Input.GetKeyDown("w") || Input.GetKeyDown("a") || Input.GetKeyDown("s") || Input.GetKeyDown("d")
            || Input.GetKeyDown("f") || Input.GetKeyDown("g")) && (State == GameState.GameOver || State == GameState.End)) {
            ResumeGame();
            ResetGame();
        }
    }

    public void UpdateGameState(GameState newState) {
        State = newState;

        switch (State) {
            case GameState.Standby :
                Standby();
                break;
            case GameState.Booting :
                Booting();
                break;
            case GameState.Start :
                StartCoroutine(SpawnBall("BallP1"));
                StartCoroutine(SpawnBall("BallP2"));
                StartCoroutine(SpawnBall("BallP3"));
                UpdateGameState(GameState.Game);
                break;
            case GameState.Game :            
                break;
            case GameState.BeforeGameOver:
                break;
            case GameState.GameOver :            
                break;
            case GameState.Victory :            
                break;
            case GameState.End :
                break;
            default:
                break;
        }
    }

    void GetBalls() {
        this.balls = new List<Ball>(FindObjectsOfType<Ball>());
    }

    void GetBumpers() {
        this.bumpers = new List<Bumper>(FindObjectsOfType<Bumper>());
    }

    void GetLights() {
        this.lights = new List<Light>(FindObjectsOfType<Light>());
    }

    void GetRigidbodies() {
        this.rigidbodies = new List<Rigidbody>(FindObjectsOfType<Rigidbody>());
    }

    void GetMeshRenderers() {
        this.meshRenderers = new List<MeshRenderer>(FindObjectsOfType<MeshRenderer>());
    }

    public void DisableAllWeakness(){
        foreach(Bumper b in bumpers){
            b.SetWeakness(false);
        }
    }

    // Changes the scene to be in a standby mode
    public void Standby(){
        StartCoroutine(StandbyCoroutine());
    }

    IEnumerator StandbyCoroutine()
    {
        standbyPlayer.gameObject.SetActive(true);
        standbyPlayer.Play();
        yield return new WaitForSeconds(.1f);

        // Audio
        AudioManager.Instance.Play("musique-inactive");

        Transform t0 = standbyPlayer.gameObject.transform;
        t0.localPosition = new Vector3(0,0,0);

        Transform t1 = bootingPlayer.gameObject.transform;
        t1.localPosition = new Vector3(0,-1f,0);
        foreach(VideoPlayer v in idlePlayers){
            t1 = v.gameObject.transform;
            t1.localPosition = new Vector3(0,-1f,0);
            v.Stop();
            v.gameObject.SetActive(false);
        }
        foreach(VideoPlayer v in tauntPlayers){
            t1 = v.gameObject.transform;
            t1.localPosition = new Vector3(0,-1f,0);
            v.Stop();
            v.gameObject.SetActive(false);
        }
        foreach(VideoPlayer v in hitPlayers){
            t1 = v.gameObject.transform;
            t1.localPosition = new Vector3(0,-1f,0);
            v.Stop();
            v.gameObject.SetActive(false);
        }
        foreach(VideoPlayer v in destructionPlayers){
            t1 = v.gameObject.transform;
            t1.localPosition = new Vector3(0,-1f,0);
            v.Stop();
            v.gameObject.SetActive(false);
        }
    }

    // Changes the scene to be in a Booting mode
    void Booting(){
        StartCoroutine(BootingCoroutine());
    }

    IEnumerator BootingCoroutine() {
        // Audio
        AudioManager.Instance.StopPlaying("musique-inactive");

        bootingPlayer.gameObject.SetActive(true);
        bootingPlayer.Play();
        bootingPlayer.loopPointReached += BootingOver;
        yield return new WaitForSeconds(.1f);

        // Audio
        AudioManager.Instance.PlayGameplayMusic();

        Transform t0 = standbyPlayer.gameObject.transform;
        Transform t1 = bootingPlayer.gameObject.transform;
        t0.position = t0.position - new Vector3(0,1,0);
        t1.position = t1.position + new Vector3(0,1,0);
        standbyPlayer.Stop();
        standbyPlayer.gameObject.SetActive(false);
    }

    // When booting is over we go on with the game automatically
    void BootingOver(UnityEngine.Video.VideoPlayer vp)
    {
        StartCoroutine(BootingOverCoroutine());
    }

    IEnumerator BootingOverCoroutine()
    {
        idlePlayers[idleIndex].gameObject.SetActive(true);
        idlePlayers[idleIndex].Play();
        yield return new WaitForSeconds(.1f);
        Transform t0 = bootingPlayer.gameObject.transform;
        Transform t1 = idlePlayers[idleIndex].gameObject.transform;
        t0.position = t0.position - new Vector3(0,1,0);
        t1.position = t1.position + new Vector3(0,1,0);
        bootingPlayer.loopPointReached -= BootingOver;
        bootingPlayer.Stop();
        bootingPlayer.gameObject.SetActive(false);
        UpdateGameState(GameState.Start);
    }

    // Balls management
    Vector3 GetBallSpawn(string ballTag) {
        Vector3 spawnPosition = Vector3.zero;

        int index = Random.Range(0, 2); // Random between 0 and 1
        
        switch(ballTag) {
            case "BallP1" :
                spawnPosition = Random.insideUnitSphere + this.spawns[0 + index].position;
                break;
            case "BallP2" :
                spawnPosition = Random.insideUnitSphere + this.spawns[2 + index].position;
                break;
            case "BallP3" :
                spawnPosition = Random.insideUnitSphere + this.spawns[4 + index].position;
                break;
            default :
                break;
        }

        spawnPosition.y = 1;

        return spawnPosition;
    }

    // Spawn a ball
    public IEnumerator SpawnBall(string ballTag, bool border = false){
        yield return new WaitForSeconds(spawnCD);
        lock(lock_1) { // Lock used to protect the number of balls
            if(this.totalBalls < this.totalMaxBalls) {
                GameObject newBall = null;
                // Which type of ball to spawn
                switch(ballTag) {
                    case "BallP1" :
                        if(this.numBallsPlayer[ballTag] < this.maxBallsByPlayer) {
                            newBall = Instantiate(this.Ball1, GetBallSpawn(ballTag), Quaternion.identity);
                            newBall.transform.parent = this.Zone1.transform;
                            newBall.GetComponent<Ball>().oldParent = this.Zone1.transform;
                            newBall.GetComponent<Ball>().originZone = 1;
                        }
                        break;
                    case "BallP2" :
                        if(this.numBallsPlayer[ballTag] < this.maxBallsByPlayer) {
                            newBall = Instantiate(this.Ball2, GetBallSpawn(ballTag), Quaternion.identity);
                            newBall.transform.parent = this.Zone2.transform;
                            newBall.GetComponent<Ball>().oldParent = this.Zone2.transform;
                            newBall.GetComponent<Ball>().originZone = 2;
                        }
                        break;
                    case "BallP3" :
                        if(this.numBallsPlayer[ballTag] < this.maxBallsByPlayer) {
                            newBall = Instantiate(this.Ball3, GetBallSpawn(ballTag), Quaternion.identity);
                            newBall.transform.parent = this.Zone3.transform;
                            newBall.GetComponent<Ball>().oldParent = this.Zone3.transform;
                            newBall.GetComponent<Ball>().originZone = 3;
                        }
                        break;
                    default :
                        break;
                }
                if(newBall != null) {
                    newBall.GetComponent<Ball>().Integration();
                    if(!border){
                        ballsUsed.transform.GetChild(ballIndex).GetComponent<MeshRenderer>().material = Instantiate(newBall.GetComponent<MeshRenderer>().material);
                        ballsUsed.transform.GetChild(ballIndex).GetComponent<MeshRenderer>().material.SetFloat("_Disintegration_Rate", 0f);
                        ballIndex++;
                    }
                    this.numBallsPlayer[ballTag]++;
                    lock(lock_2) { // Lock used to protect the number of balls
                        this.totalBalls++;
                    }
                    GetBalls();
                }
            }
        }
    }

    IEnumerator SpawnBossBall() {
        GameObject bossBall = Instantiate(this.ballBoss, new Vector3(0, 30, 97), Quaternion.identity);
        bossBall.GetComponent<BallBoss>().borders = GameObject.Find("Borders").transform;
        bossBall.transform.parent = this.boss;

        while(bossBall.transform.position.y > 1.5) {
            Vector3 newPos = bossBall.transform.position;
            newPos.y -= 1f;

            bossBall.transform.position = newPos;

            yield return new WaitForSeconds(0.00001f);
        }

        bossBall.GetComponent<BallBoss>().StartBossSequence();
    }

    // Kill a ball
    public void DeathBall(GameObject ball, bool border) {
        lock(lock_1) {
            ball.GetComponent<Ball>().Desintegration();
            this.balls.Remove(ball.GetComponent<Ball>());
            if(!border) {                
                deadBalls++;
            } else {
                this.totalBalls--;
            }
            this.numBallsPlayer[ball.tag]--;
            if(this.numBallsPlayer[ball.tag] == 0) {
                StartCoroutine(SpawnBall(ball.tag, border));
            }
            if(deadBalls == 15 && State == GameState.Game){
                if(shakeCoroutine != null){
                    StopCoroutine(shakeCoroutine);
                }
                terrainToShake[0].transform.position = new Vector3(0f, 45f, 0f);
                terrainToShake[0].transform.eulerAngles = new Vector3(90, 0, 0);
                GameOver();
            }
        }        
    }

    // Score management
    public void AddScore(string ballTag, int scoreAmount) {
        lock(score_lock) {
            switch(ballTag) {
                case "BallP1" :
                    this.scores[0] += scoreAmount;
                    this.scoresText[0].text = "J1\n" + this.scores[0].ToString();
                    break;
                case "BallP2" :
                    this.scores[1] += scoreAmount;
                    this.scoresText[1].text = "J2\n" + this.scores[1].ToString();
                    break;
                case "BallP3" :
                    this.scores[2] += scoreAmount;
                    this.scoresText[2].text = "J3\n" + this.scores[2].ToString();
                    break;
                default :
                    break;
            }
        }
    }

    // When attack is over, if boss has not taunted yet this phase then it taunts and plazs special effects such as smoke. If it has already taunted then we put the idle video
    void AttackOver(UnityEngine.Video.VideoPlayer vp)
    {
        StartCoroutine(AttackOverCoroutine());
    }

    IEnumerator AttackOverCoroutine()
    {
        if(taunted){
            idlePlayers[idleIndex].gameObject.SetActive(true);
            idlePlayers[idleIndex].Play();
            yield return new WaitForSeconds(.1f);
            Transform t0 = hitPlayers[hitIndex+localHitIndex].gameObject.transform;
            Transform t1 = idlePlayers[idleIndex].gameObject.transform;
            t0.position = t0.position - new Vector3(0,1,0);
            t1.position = t1.position + new Vector3(0,1,0);
            hitPlayers[hitIndex+localHitIndex].loopPointReached -= AttackOver;
            hitPlayers[hitIndex+localHitIndex].Stop();
            hitPlayers[hitIndex+localHitIndex].gameObject.SetActive(false);
            playing = false;
            localHitIndex=localHitIndex+1%2;
            if(idleIndex < 5)
            {
                this.canWeaknessSet = true;
            }
        }
        else{
            tauntPlayers[tauntIndex].gameObject.SetActive(true);
            tauntPlayers[tauntIndex].Play();
            tauntPlayers[tauntIndex].loopPointReached += TauntOver;
            yield return new WaitForSeconds(.1f);

            // Audio
            // Taunt sounds
            AudioManager.Instance.PlayTauntSound(tauntIndex);

            Transform t0 = hitPlayers[hitIndex+localHitIndex].gameObject.transform;
            Transform t1 = tauntPlayers[tauntIndex].gameObject.transform;
            t0.position = t0.position - new Vector3(0,1,0);
            t1.position = t1.position + new Vector3(0,1,0);
            if(tauntEffectCount%3 == 0){
                this.smokeSpline.SetSmoke();
            }else{
                this.lightSpline.SetLight();
            }
            hitPlayers[hitIndex+localHitIndex].loopPointReached -= AttackOver;
            hitPlayers[hitIndex+localHitIndex].Stop();
            hitPlayers[hitIndex+localHitIndex].gameObject.SetActive(false);
            localHitIndex=localHitIndex+1%2;
            tauntEffectCount++;
        }
    }

    // When the taunt video is over we play the idle video of the corresponding index
    void TauntOver(UnityEngine.Video.VideoPlayer vp) {
        StartCoroutine(TauntOverCoroutine());
    }

    IEnumerator TauntOverCoroutine() {
        this.smokeSpline.StopSplineSmoke();
        this.lightSpline.StopSplineLight();
        idlePlayers[idleIndex].gameObject.SetActive(true);
        idlePlayers[idleIndex].Play();
        yield return new WaitForSeconds(.1f);
        Transform t0 = tauntPlayers[tauntIndex].gameObject.transform;
        Transform t1 = idlePlayers[idleIndex].gameObject.transform;
        t0.position = t0.position - new Vector3(0,1,0);
        t1.position = t1.position + new Vector3(0,1,0);
        tauntPlayers[tauntIndex].loopPointReached -= TauntOver;
        tauntPlayers[tauntIndex].Stop();
        tauntPlayers[tauntIndex].gameObject.SetActive(false);
        if(tauntIndex == 4){
            tauntIndex++;
        }else{
            taunted = true;
        }
        this.canWeaknessSet = true;
        playing = false;
    }

    // When the destruction video is over, we play the idle video of the next index, if we are at the final index (5 here) then we spawn boss ball
    void DestructionOver(UnityEngine.Video.VideoPlayer vp) {
        StartCoroutine(DestructionOverCoroutine());
    }

    IEnumerator DestructionOverCoroutine() {
        idleIndex++;
        Debug.Log("IdleIndex " + idleIndex);
        
        idlePlayers[idleIndex].gameObject.SetActive(true);
        idlePlayers[idleIndex].Play();
        yield return new WaitForSeconds(.1f);
        Transform t0 = destructionPlayers[destructionIndex].gameObject.transform;
        Transform t1 = idlePlayers[idleIndex].gameObject.transform;
        t0.position = t0.position - new Vector3(0,1,0);
        t1.position = t1.position + new Vector3(0,1,0);
        destructionPlayers[destructionIndex].loopPointReached -= DestructionOver;
        destructionPlayers[destructionIndex].Stop();
        destructionPlayers[destructionIndex].gameObject.SetActive(false);
        
        destructionIndex++;
        hitIndex+=2;
        localHitIndex=0;
        tauntIndex++;
        thresholdIndex++;
        playing = false;
        taunted = false;
        if(idleIndex == 5){
            StartCoroutine(SpawnBossBall());
            playing = true;
        } else {
            this.canWeaknessSet = true;
        }
    }
    // Fade to white animation at the end of the game
    public IEnumerator FadeWhiteCoroutine() {
        // Audio
        AudioManager.Instance.FadingAll();
        AudioManager.Instance.PlayVictoryMusic();

        if (profile.TryGet<ColorAdjustments>(out var ca)) {
            for(int i = 0; i < 100; i++) {
                ca.postExposure.value += 0.2f;
                yield return new WaitForSeconds(0.05f);
            }
            UpdateGameState(GameState.End);
            PauseGame();            
        }
    }

    // When boss is attacked, we remove the idle video player and show either a destruction or a hit video depending on the hp and its treshold
    public void AttackBoss() {
        StartCoroutine(AttackBossCoroutine());
    }

    IEnumerator AttackBossCoroutine() {
        if(!playing){
            playing = true;
            this.bossLife -= 25;
            if(thresholds[thresholdIndex] >= bossLife) {
                if (idleIndex == 4) {
                    UpdateGameState(GameState.Victory);
                }
                destructionPlayers[destructionIndex].gameObject.SetActive(true);
                destructionPlayers[destructionIndex].Play();
                destructionPlayers[destructionIndex].loopPointReached += DestructionOver;
                yield return new WaitForSeconds(.1f);

                // Audio
                // Destruction Sounds
                AudioManager.Instance.PlayDestructionSound(destructionIndex);

                if(destructionIndex == destructionPlayers.Length - 1) {
                    AudioManager.Instance.StopGameplayMusic();
                }

                Transform t0 = destructionPlayers[destructionIndex].gameObject.transform;
                Transform t1 = idlePlayers[idleIndex].gameObject.transform;
                t0.position = t0.position + new Vector3(0,1,0);
                t1.position = t1.position - new Vector3(0,1,0);
                idlePlayers[idleIndex].Stop();
                idlePlayers[idleIndex].gameObject.SetActive(false);
            } else {
                if(!shaked && idleIndex == 1){
                    ShakeTerrain();
                    shaked = true;
                }
                hitPlayers[hitIndex+localHitIndex].gameObject.SetActive(true);
                hitPlayers[hitIndex+localHitIndex].Play();
                hitPlayers[hitIndex+localHitIndex].loopPointReached += AttackOver;
                yield return new WaitForSeconds(.1f);

                // Audio
                // Hit sounds
                AudioManager.Instance.PlayHitSound(hitIndex+localHitIndex);

                Transform t0 = hitPlayers[hitIndex+localHitIndex].gameObject.transform;
                Transform t1 = idlePlayers[idleIndex].gameObject.transform;
                t0.position = t0.position + new Vector3(0,1,0);
                t1.position = t1.position - new Vector3(0,1,0);
                idlePlayers[idleIndex].Stop();
                idlePlayers[idleIndex].gameObject.SetActive(false);
                FlashLights();
            }
        }
    }
    //Flashing all lights
    void FlashLights(){
        foreach(Light l in lights){
            if(l.enabled){
                StartCoroutine(FlashLightsCoroutine(l));
            }
        }
    }

    IEnumerator FlashLightsCoroutine(Light l){
        for(int i = 0; i < 10; i++){
            l.enabled = false;
            yield return new WaitForSeconds(0.05f);
            l.enabled = true;
            yield return new WaitForSeconds(0.05f);
        }
    }
    // Shake the terrain via the cameras, jsut applying a translate and changing orientation by looking at the center of the terrain
    void ShakeTerrain(){
        foreach(Camera c in terrainToShake){
            shakeCoroutine = StartCoroutine(ShakeTerrainCoroutine(c));
        }
    }

    IEnumerator ShakeTerrainCoroutine(Camera c){
        for(int i = 0; i < 75; i++){
            c.transform.Translate(-0.3f, 0.0f, 0.0f, Space.World);
            c.transform.LookAt(center.transform, c.transform.up);
            yield return 0.01;
        }
        yield return new WaitForSeconds(4f);
        for(int i = 0; i < 75; i++){
            c.transform.Translate(0.3f, 0.0f, 0.0f, Space.World);
            c.transform.LookAt(center.transform, c.transform.up);
            yield return 0.01;
        }
        yield return new WaitForSeconds(.01f);
        for(int i = 0; i < 75; i++){
            c.transform.Translate(0.3f, 0.0f, 0.0f, Space.World);
            c.transform.LookAt(center.transform, c.transform.up);
            yield return 0.01;
        }
        yield return new WaitForSeconds(4f);
        for(int i = 0; i < 75; i++){
            c.transform.Translate(-0.3f, 0.0f, 0.0f, Space.World);
            c.transform.LookAt(center.transform, c.transform.up);
            yield return 0.01;
        }
    }
    // when the game is over we disable the rigidbodies so we can rotate game objects
    void GameOver(){
        foreach(Rigidbody r in rigidbodies) {
            r.isKinematic = true;
        }
        foreach(GameObject g in gameOverTurn) {
            StartCoroutine(GameOverCoroutine(g));            
        }
    }

    IEnumerator GameOverCoroutine(GameObject g){
        UpdateGameState(GameState.BeforeGameOver);
        // Audio
        AudioManager.Instance.PlayGameOverMusic();
        for(int i = 0; i < 270; i++){
            float pos = -(i*(2.0f/3.0f))/3;
            g.transform.localPosition = new Vector3(pos,pos,0f);
            g.transform.Rotate(0.0f,0.0f,.33f);
            yield return new WaitForSeconds(0.01f);
        }
        UpdateGameState(GameState.GameOver);
    }
    // Changing all materials to a gray base material by unity
    void ChangeToBaseMat(){
        foreach(MeshRenderer m in meshRenderers){
            StartCoroutine(ChangeToBaseMatCoroutine(m));
        }
    }

    IEnumerator ChangeToBaseMatCoroutine(MeshRenderer m){
        Debug.Log(m.materials.Length.ToString());
        Material[] mats = m.materials;
        for(int i = 0; i < mats.Length; i++){
            mats[i] = baseMat;
        }
        m.materials = mats;
        yield return 0;
    }

    // Admin shortcuts
    void ResetBalls() {
        Debug.Log("Reset Ball");
        for(int i = 0; i < this.balls.Count ; i++) {
            this.balls[i].transform.parent = this.balls[i].oldParent;
            this.balls[i].transform.localPosition = new Vector3(0f, 1f, 0f);
            this.balls[i].speed = Vector3.zero;
            this.balls[i].inSlide = false;
            this.balls[i].isInPortal = false;
        }
    }

    void ResetScore() {
        Debug.Log("Reset Score");
        for(int i = 0; i < this.scores.Length ; i++) {
            this.scores[i] = 0;
        }
    }

    void ResetGame() {
        Debug.Log("Reset Game");
        // Reload master
        SceneManager.LoadScene(0);
    }

    void PauseGame() {
        Debug.Log("Pause game");
        Time.timeScale = 0;
    }

    void ResumeGame() {
        Debug.Log("Resume game");
        Time.timeScale = 1;
    }
}