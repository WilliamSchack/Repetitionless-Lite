using UnityEngine;

[CreateAssetMenu]
public class RepetitionlessMaterialDataSO : ScriptableObject
{
    private Texture2D _dataTexture; 

    public void UpdateVariable(int propertyIndex, int layerIndex)
    {
        int texCoordX = propertyIndex + 1;
        int texCoordY = layerIndex + 1;
    }
}