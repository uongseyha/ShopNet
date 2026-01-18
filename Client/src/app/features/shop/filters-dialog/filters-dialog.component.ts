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
  private dialogRef = inject(MatDialogRef<FiltersDialogComponent>);
  data = inject(MAT_DIALOG_DATA);
  selectedBrands: string[] = this.data.selectedBrands || [];
  selectedTypes: string[] = this.data.selectedTypes || [];

  applyFilters() {
    this.dialogRef.close({
      selectedBrands: this.selectedBrands,
      selectedTypes: this.selectedTypes
    });
  }
}
