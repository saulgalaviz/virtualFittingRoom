using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ExceptionsLogger : MonoBehaviour 
{
	[SerializeField]Text textField;
	[SerializeField]float entryLifetime = 5f;
	LinkedList<Entry> stringsList = new LinkedList<Entry>();

	class Entry
	{
		public string text;
		public float remainingTime;
	}

	void Start () 
	{
		textField.text = "";
	}

	public void AddEntry(string txt)
	{
		Entry newEntry = new Entry();
		newEntry.text = txt;
		newEntry.remainingTime = entryLifetime;
		stringsList.AddFirst(newEntry);
	}

	void Update () 
	{
		if (stringsList.Count > 0)
		{
			ProcessEntries();
		}
		else
		{
			textField.text = "";
		}
	}

	void ProcessEntries()
	{
    const int MAX_ENTRIES_LENGTH = 5000;
		StringBuilder sb = new StringBuilder();
		LinkedListNode<Entry> currentNode, nextNode;

		currentNode = stringsList.Last;
		while (currentNode != null)
		{
			nextNode = currentNode.Previous;
			sb.Append(currentNode.Value.text);
			if (nextNode != null) sb.AppendLine();
			currentNode.Value.remainingTime -= Time.deltaTime;
			if (currentNode.Value.remainingTime < 0) stringsList.Remove(currentNode);
			currentNode = nextNode;
      if (sb.Length > MAX_ENTRIES_LENGTH)
      {
        sb.Remove(MAX_ENTRIES_LENGTH, sb.Length - MAX_ENTRIES_LENGTH);
        break;
      }
		}
		textField.text = sb.ToString();
	}
}