import { Component, inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ShopParams } from '../../../shared/models/shopParams';
import { ShopService } from '../../../core/services/shop.service';
import { MatDivider } from "@angular/material/divider";
import { MatCardModule } from '@angular/material/card';
import { FormsModule } from '@angular/forms';
import {MatListOption, MatSelectionList} from '@angular/material/list';
import { MatButton } from '@angular/material/button';

@Component({
  selector: 'app-filters-dialog',
  imports: [
    MatDivider,
    MatSelectionList,
    MatListOption,
    MatButton,
    FormsModule
],
  templateUrl: './filters-dialog.component.html',
  styleUrl: './filters-dialog.component.css',
})
export class FiltersDialogComponent{
  shopService = inject(ShopService);
  dialogRef = inject(MatDialogRef<FiltersDialogComponent>);
  data = inject(MAT_DIALOG_DATA);
  selectedBrands: string[] = [...(this.data.selectedBrands || [])];
  selectedTypes: string[] = [...(this.data.selectedTypes || [])];

  toggleBrand(brand: string) {
    const index = this.selectedBrands.indexOf(brand);
    if (index > -1) {
      this.selectedBrands.splice(index, 1);
    } else {
      this.selectedBrands.push(brand);
    }
  }

  toggleType(type: string) {
    const index = this.selectedTypes.indexOf(type);
    if (index > -1) {
      this.selectedTypes.splice(index, 1);
    } else {
      this.selectedTypes.push(type);
    }
  }

  isSelected(item: string, type: 'brand' | 'type'): boolean {
    return type === 'brand' 
      ? this.selectedBrands.includes(item)
      : this.selectedTypes.includes(item);
  }

  applyFilters() {
    this.dialogRef.close({
      selectedBrands: this.selectedBrands,
      selectedTypes: this.selectedTypes
    });
  }

  clearFilters() {
    this.selectedBrands = [];
    this.selectedTypes = [];
  }
}
