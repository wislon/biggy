using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Biggy.Core;
using Biggy.Data.Json;
using NUnit.Framework;

namespace Tests {
  [TestFixture()]
  [Category("Biggy List with JSON Store")]
  public class BiggyListWithJsonStore {
    private JsonStore<Property> _propertyStore;

    [SetUp]
    public void init() {
      _propertyStore = new JsonStore<Property>();
      string path = _propertyStore.DbPath;
      File.Delete(path);
    }

    [Test()]
    public void BiggyList_Initializes_From_Empty_Table() {
      var propertyList = new BiggyList<Property>(_propertyStore);
      Assert.True(propertyList.Store != null && propertyList.Count == 0);
    }

    [Test()]
    public void Biggylist_Adds_Single_Item_To_Json_Store() {
      var propertyList = new BiggyList<Property>(_propertyStore);
      int initialCount = propertyList.Count;

      var newProperty = new Property { Id = 1, Name = "Watergate Apartments", Address = "2639 I St NW, Washington, D.C. 20037" };
      propertyList.Add(newProperty);

      // Reload from the store:
      propertyList = new BiggyList<Property>(_propertyStore);

      var addedItems = propertyList.FirstOrDefault(p => p.Name.Contains("Watergate"));
      Assert.IsTrue(initialCount == 0 && propertyList.Count == 1);
    }

    [Test()]
    public void Biggylist_Adds_Range_Of_Items_To_Json_Store() {
      var propertyList = new BiggyList<Property>(_propertyStore);
      int initialCount = propertyList.Count;

      var myBatch = new List<Property>();
      int qtyToAdd = 10;

      // This test uses an initial index value of 1 so taht there is a non-zero ID for the first item:
      for (int i = 1; i <= qtyToAdd; i++) {
        var newProperty = new Property { Id = i, Name = "New Apartment " + i, Address = "Some Street in a Lonely Town" };
        myBatch.Add(newProperty);
      }
      propertyList.Add(myBatch);

      // Reload from the store:
      propertyList = new BiggyList<Property>(_propertyStore);

      var addedItems = propertyList.FirstOrDefault(p => p.Name.Contains("New Apartment"));
      Assert.IsTrue(initialCount == 0 && propertyList.Count == qtyToAdd);
    }

    [Test()]
    public void Biggylist_Updates_Single_Item_In_Json_Store() {
      var propertyList = new BiggyList<Property>(_propertyStore);
      int initialCount = propertyList.Count;

      var newProperty = new Property { Id = 1, Name = "John's Luxury Apartments", Address = "2639 I St NW, Washington, D.C. 20037" };
      propertyList.Add(newProperty);

      int addedItemId = newProperty.Id;

      // Just to be sure, reload from backing store and check what was added:
      propertyList = new BiggyList<Property>(_propertyStore);
      var addedProperty = propertyList.FirstOrDefault(p => p.Id == addedItemId);
      bool isAddedProperly = addedProperty.Name.Contains("John's Luxury");

      // Now Update:
      string newName = "John's Low-Rent Apartments";
      addedProperty.Name = newName;
      propertyList.Update(addedProperty);

      // Go fetch again:
      propertyList = new BiggyList<Property>(_propertyStore);
      addedProperty = propertyList.FirstOrDefault(p => p.Id == addedItemId);
      bool isUpdatedProperly = addedProperty.Name == newName;

      Assert.IsTrue(isAddedProperly && isUpdatedProperly);
    }

    [Test()]
    public void Biggylist_Updates_Range_Of_Items_In_Json_Store() {
      var propertyList = new BiggyList<Property>(_propertyStore);
      int initialCount = propertyList.Count;

      var myBatch = new List<Property>();
      int qtyToAdd = 10;

      // This test uses an initial index value of 1 so taht there is a non-zero ID for the first item:
      for (int i = 1; i <= qtyToAdd; i++) {
        myBatch.Add(new Property { Id = i, Name = "John's Luxury Townhomes" + i, Address = i + " Property Parkway, Portland, OR 97204" });
      }
      propertyList.Add(myBatch);

      // Just to be sure, reload from backing store and check what was added:
      propertyList = new BiggyList<Property>(_propertyStore);
      int addedCount = propertyList.Count;

      // Update each item:
      for (int i = 0; i < qtyToAdd; i++) {
        propertyList.ElementAt(i).Name = "John's Low-Rent Brick Homes " + i;
      }
      propertyList.Update(propertyList.ToList());

      // Reload, and check updated names:
      propertyList = new BiggyList<Property>(_propertyStore);
      var updatedItems = propertyList.Where(p => p.Name.Contains("Low-Rent Brick"));
      Assert.IsTrue(updatedItems.Count() == qtyToAdd);
    }

    [Test()]
    public void Biggylist_Removes_Single_Item_From_Json_Store() {
      var propertyList = new BiggyList<Property>(_propertyStore);
      int initialCount = propertyList.Count;

      var newProperty = new Property { Id = 1, Name = "John's High-End Apartments", Address = "16 Property Parkway, Portland, OR 97204" };
      propertyList.Add(newProperty);

      int addedItemId = newProperty.Id;

      // Just to be sure, reload from backing store and check what was added:
      propertyList = new BiggyList<Property>(_propertyStore);
      var addedProperty = propertyList.FirstOrDefault(p => p.Id == addedItemId);
      bool isAddedProperly = addedProperty.Name.Contains("High-End");

      int qtyAdded = propertyList.Count;

      // Delete:
      var foundProperty = propertyList.FirstOrDefault();
      propertyList.Remove(foundProperty);

      propertyList = new BiggyList<Property>(_propertyStore);
      int remaining = propertyList.Count;
      Assert.IsTrue(isAddedProperly && qtyAdded == 1 && remaining == 0);
    }

    [Test()]
    public void Biggylist_Removes_Range_Of_Items_From_Json_Store() {
      var propertyList = new BiggyList<Property>(_propertyStore);
      int initialCount = propertyList.Count;

      var myBatch = new List<Property>();
      int qtyToAdd = 10;

      // This test uses an initial index value of 1 so taht there is a non-zero ID for the first item:
      for (int i = 1; i <= qtyToAdd; i++) {
        myBatch.Add(new Property { Id = i, Name = "Boardwalk " + i, Address = i + " Property Parkway, Portland, OR 97204" });
      }
      propertyList.Add(myBatch);

      // Re-load from back-end:
      propertyList = new BiggyList<Property>(_propertyStore);
      int qtyAdded = propertyList.Count;

      // Select 5 for deletion:
      int qtyToDelete = 5;
      var deleteThese = propertyList.Where(c => c.Id <= qtyToDelete);
      propertyList.Remove(deleteThese);

      // Delete:
      propertyList = new BiggyList<Property>(_propertyStore);
      int remaining = propertyList.Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == qtyAdded - qtyToDelete);
    }

    [Test()]
    public void Biggylist_Removes_All_Items_From_Json_Store() {
      var propertyList = new BiggyList<Property>(_propertyStore);
      int initialCount = propertyList.Count;

      var myBatch = new List<Property>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        myBatch.Add(new Property { Id = i, Name = "Marvin Gardens " + i, Address = i + " Property Parkway, Portland, OR 97204" });
      }
      propertyList.Add(myBatch);

      // Re-load from back-end:
      propertyList = new BiggyList<Property>(_propertyStore);
      int qtyAdded = propertyList.Count;

      propertyList.Clear();

      // Delete:
      propertyList = new BiggyList<Property>(_propertyStore);
      int remaining = propertyList.Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == 0);
    }
  }
}