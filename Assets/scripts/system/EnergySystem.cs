class EnergySystem {
    public static bool CheckEnergy(Player pl, int val) {
        return pl.energy >= val;
    }

    public static void WarnEnergy(Player pl, int val) {
        ChatManager.Instance.SendLocalComment("<color=\"red\"> 에너지가 " + (val - pl.energy).ToString() + " 만큼 부족합니다.</color>");
    }

    public static bool CheckNWarn(Player pl, int val) {
        if (CheckEnergy(pl, val)) {
            return true;
        } else {
            WarnEnergy(pl, val);
            return false;
        }
    }
}