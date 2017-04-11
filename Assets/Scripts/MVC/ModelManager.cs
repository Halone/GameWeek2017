using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class ModelManager: BaseManager<ModelManager> {
    #region Variables
    private const string FIELD_X        = "x";
    private const string FIELD_Y        = "y";
    private const string FIELD_Z        = "z";
    private const string FIELD_W        = "w";
    private const string FIELD_TYPE     = "type";
    private const string FIELD_SUBTYPE  = "subtype";
    private const string FIELD_ROTATION = "rotation";
    private const string FIELD_SCALE_X  = "scale_x";
    private const string FIELD_SCALE_Y  = "scale_y";
    private const string FIELD_SCALE_Z  = "scale_z";
    private const string CELL_DECOR     = "decor";
    private const string CELL_OBSTACLE  = "obstacle";
    private const string CELL_TILE      = "tile";
    private const string CELL_OBJECT    = "object";

    public Action<float> onCreateProgress;
    public Action<Dictionary<Vector2, Cell>, List<List<Vector2>>, List<List<DecorObject>>> onCreateDone;
    #endregion

    #region Initialisation & Destroy
    protected override IEnumerator CoroutineStart() {
        yield return true;
        isReady = true;
    }

    protected override void Destroy() {
        onCreateDone = null;

        base.Destroy();
    }
    #endregion

    #region Model Managment
    protected override void Play(int p_LevelID, int p_PhaseManager) {
        CreatModel(DataManager.instance.GetLevel(p_LevelID));
    }

    private void CreatModel(JSONObject l_JsonModel) {
        Dictionary<Vector2, Cell> l_Model       = new Dictionary<Vector2, Cell>();
        List<List<Vector2>> l_Parts             = new List<List<Vector2>>();
        List<List<DecorObject>> l_DecorObjects  = new List<List<DecorObject>>();
        float l_Percent                         = 100 / Mathf.Max(l_JsonModel.Count, 1);

        for (int cptPart = 0; cptPart < l_JsonModel.Count; cptPart++) {
            List<JSONObject> l_JsonPart = l_JsonModel[cptPart].list;
            List<Vector2> l_Part        = new List<Vector2>();
            List<DecorObject> l_Decors  = new List<DecorObject>();
            float l_SubPercent          = l_Percent / l_JsonPart.Count;

            for (int cptCell = 0; cptCell < l_JsonPart.Count; cptCell++) {
                JSONObject l_JsonCell   = l_JsonPart[cptCell];
                Vector2 l_PosModel      = new Vector2(l_JsonCell.GetField(FIELD_X).f, l_JsonCell.GetField(FIELD_Z).f);
                Cell l_Cell;

                switch (l_JsonCell.GetField(FIELD_TYPE).str) {
                    case CELL_OBJECT:
                        JSONObject l_Rotation = l_JsonCell.GetField(FIELD_ROTATION);
                        l_Decors.Add(new DecorObject(
                            l_JsonCell.GetField(FIELD_SUBTYPE).str,
                            new Vector3(l_JsonCell.GetField(FIELD_X).f, l_JsonCell.GetField(FIELD_Y).f, l_JsonCell.GetField(FIELD_Z).f),
                            new Quaternion(l_Rotation.GetField(FIELD_X).f, l_Rotation.GetField(FIELD_Y).f, l_Rotation.GetField(FIELD_Z).f, l_Rotation.GetField(FIELD_W).f),
                            new Vector3(l_JsonCell.GetField(FIELD_SCALE_X).f, l_JsonCell.GetField(FIELD_SCALE_Y).f, l_JsonCell.GetField(FIELD_SCALE_Z).f)
                        ));
                        break;
                    case CELL_OBSTACLE:
                        l_Cell = new Cell(CELL_OBSTACLE, false, false);
                        l_Model.Add(l_PosModel, l_Cell);
                        l_Part.Add(l_PosModel);
                        break;
                    case CELL_TILE:
                        l_Cell = new Tile(CELL_TILE, l_PosModel, l_JsonCell);
                        l_Model.Add(l_PosModel, l_Cell);
                        l_Part.Add(l_PosModel);
                        break;
                    default:
                        l_Cell = new Cell(CELL_DECOR, false, true);
                        l_Model.Add(l_PosModel, l_Cell);
                        l_Part.Add(l_PosModel);
                        break;
                }
                
                if (onCreateProgress != null) onCreateProgress(l_SubPercent);
            }
            
            l_Parts.Add(l_Part);
            l_DecorObjects.Add(l_Decors);
        }
        
        if (onCreateDone != null) onCreateDone(l_Model, l_Parts, l_DecorObjects);
    }
    #endregion
}