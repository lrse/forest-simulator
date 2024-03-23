using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class EndNode : BiomeNode {

    [Input] public Texture2D texture;
    [HideInInspector] public Texture2D output;

    public override object GetValue(NodePort port) {
        output = texture;
        return output;
    }

    public override void OnInputChanged() {
        output = GetInputValue<Texture2D>("texture", this.texture);
    }

    public Texture2D GetTexture() {
        OnInputChanged();
        return output;
    }

    public NodePort GetInputPort() {
        // CAREFUL: works because there's only one
        foreach (NodePort port in Inputs) {
            if (port != null) return port;
        }
        return null;
    }
}
