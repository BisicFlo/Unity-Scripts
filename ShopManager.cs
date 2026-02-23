using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour {

    [SerializeField] List<Color> RarityColors = new List<Color>(); // =>  replaced by background images list  

    [SerializeField] ItemDatabase itemDatabase;

    [SerializeField] List<Button> buttonList = new List<Button>();

    [SerializeField] Button RefreshButton;

    private static readonly int[,] weights =
       {{ 100,  0,  0,  0,  0 },  // Lvl 1
        {  75, 25,  0,  0,  0 },  // Lvl 2
        {  55, 30, 15,  0,  0 },  // Lvl 3
        {  45, 33, 20,  2,  0 },  // Lvl 4
        {  30, 40, 25,  5,  0 },  // Lvl 5
        {  16, 30, 43, 10,  1 },  // Lvl 6
        {  15, 20, 32, 30,  3 },  // Lvl 7
        {  10, 17, 25, 33, 15 },  // Lvl 8
        {   5, 10, 20, 40, 25 },  // Lvl 9
        {   1,  2, 12, 50, 35 }}; // Lvl 10

    //private int[] price = { 1, 2, 3, 4, 5 };

    private static readonly int[] xpRequired = { 2, 2, 6, 10, 20, 36, 60, 68, 80 };

    private int RerollPrice = 2;

    [SerializeField] private int playerLevel ; //Player level -> create PlayerData
    [SerializeField] private int xp; //Player level -> create PlayerData
    [SerializeField] private int PlayerMoney = 10; //Player money -> create PlayerData

    private int RefreshCost = 1;
    private bool firstTimeShop = true;
    //private int rarity; // 1:Common | 2:Uncommon | 3:Rare | 4:Epic | 5:Legendary

    private void OnEnable() {
        if (firstTimeShop) {
            firstTimeShop = false;
            ClearAllButtons();
            StartCoroutine(SetupAllButtons());
            //subscribe to RefreshButton 
        }
    }
    private void GainXp(int xpGained) {
        if (playerLevel == 10) return; //Niveau Max

        xp += xpGained;
        if (xp >= xpRequired[playerLevel - 1]) {
            xp = 0;
            playerLevel++;
        }
    }

    private void GainMoney(int money) {
        PlayerMoney += money;
    }

    private bool BuySomething(int price) {
        if (PlayerMoney < price) {
            return false; // Not enough money
        } else {
            PlayerMoney -= price;
            return true;
        }
    }

    private int PickRarity(int level) {

        int rarity = 1;
        int randomNumber = Random.Range(1, 101); // (int minInclusive, int maxExclusive);
        int length = weights.GetLength(1); // => 5

        for (int i = 0; i < length; i++) {
            randomNumber -= weights[level - 1, i];
            if (randomNumber <= 0) return rarity;
            rarity++;
        }

        return -1;
    }

    private TurretData PickTurret(int rarity) {
        return itemDatabase.GetRandomTurretFromRarity(rarity);
    }

    private void ChangeColorButtonFromRarity(Button button, int rarity) {
        button.image.color = RarityColors[rarity - 1];
    }
    private void ChangePriceButton(Button button, int price) {
        button.transform.GetChild(0).GetComponent<Text>().text = price.ToString();
    }

    private void SetupOneButton(Button button) {
        int rarity = PickRarity(playerLevel);
        Debug.Log("rarity : " + rarity);
        TurretData selectedTurret = PickTurret(rarity);
        ChangeColorButtonFromRarity(button, rarity);
        ChangePriceButton(button, selectedTurret.Cost);
        button.gameObject.SetActive(true);
    }


    private IEnumerator SetupAllButtons() {
        
        foreach (var button in buttonList) {
            yield return new WaitForSeconds(1);

            SetupOneButton(button);
        }
        yield return new WaitForSeconds(1);
        RefreshButton.gameObject.SetActive(true);

    }

    private void ClearAllButtons() {
        foreach (var button in buttonList) {
            button.gameObject.SetActive(false); 
        }
    }

    public void RefreshShop() {
        if (BuySomething(RefreshCost)) {
            RefreshButton.gameObject.SetActive(false);
                     
            ClearAllButtons();
            StartCoroutine(SetupAllButtons());

        }
    }


}
