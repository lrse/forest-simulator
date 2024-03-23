using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[NodeWidth(298), ShowPreview]
public abstract class BiomeNode : Node {
	public Action onStateChange;

	public void SendSignal(NodePort output) {
		// Loop through port connections
		int connectionCount = output.ConnectionCount;
		for (int i = 0; i < connectionCount; i++) {
			NodePort connectedPort = output.GetConnection(i);

			// Get connected ports logic node
			BiomeNode connectedNode = connectedPort.node as BiomeNode;

			// Trigger it
			if (connectedNode != null) connectedNode.OnInputChanged();
		}
		if (onStateChange != null) onStateChange();
	}

	public abstract void OnInputChanged();

	public override void OnCreateConnection(NodePort from, NodePort to) {
		OnInputChanged();
	}

    public NodePort GetOutputPort() {
        // CAREFUL: works because there's only one
        foreach (NodePort port in Outputs) {
            if (port != null) return port;
        }
        return null;
    }
}



[System.AttributeUsage(System.AttributeTargets.Class)]
public class ShowPreviewAttribute : System.Attribute {
	public bool showPreview;

	public ShowPreviewAttribute(bool showPreview = true) {
		this.showPreview = showPreview;
	}
}